using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Serilog;

namespace Asypi {
    /// <summary>A file path with a frequency attached.</summary>
    class FFPair {
        public string FilePath { get; set; }
        public long Frequency { get; set; }
        
        public FFPair(string filePath) {
            FilePath = filePath;
            Frequency = 1;
        }
    }
    
    /// <summary>A comparer for <see cref="FFPair" />.</summary>
    class FFPairComparer : IComparer<FFPair> {
        public int Compare(FFPair a, FFPair b) {
            if (a.Frequency == b.Frequency) {
                return 0;
            } else if (a.Frequency < b.Frequency) {
                return -1;
            } else {
                return 1;
            }
        }
    }
    
    /// <summary>A reverse-order comparer for <see cref="FFPair" />.</summary>
    class FFPairComparerReverse : IComparer<FFPair> {
        public int Compare(FFPair a, FFPair b) {
            if (a.Frequency == b.Frequency) {
                return 0;
            } else if (a.Frequency < b.Frequency) {
                return 1;
            } else {
                return -1;
            }
        }
    }
    
    /// <summary>A utility class for storing and serving files. Minimizes disk reads via an LFU cache.</summary>
    public static class FileServer {
        // LFU cache
        
        static object lck = new object();
        
        static Dictionary<string, byte[]> contentByFile = new Dictionary<string, byte[]>();
        static List<FFPair> frequencyByFile = new List<FFPair>();
        
        static long MAX_CACHE_SIZE_IN_BYTES;
        static long MAX_FILE_SIZE_IN_BYTES;
        
        static FFPairComparerReverse comparer = new FFPairComparerReverse();
        
        
        public static void Init() {
            MAX_CACHE_SIZE_IN_BYTES = Params.FileServerLFUCacheSize * Params.BYTES_PER_MIB;
            MAX_FILE_SIZE_IN_BYTES = MAX_CACHE_SIZE_IN_BYTES / 2;
            
            // service worker
            Task.Run(async () => {
                while (true) {
                    await Task.Delay(Params.FILESERVER_EPOCH_LENGTH);
                    
                    lock (lck) {
                        // end of epoch, halve each frequency so that values
                        // stabilize in the long-term
                        
                        foreach (FFPair pair in frequencyByFile) {
                            pair.Frequency /= 2;
                        }
                    }
                    
                    await UpdateCache();
                }
            });
        }
        
        /// <summary>
        /// Resets LFU cache files and frequency counters.
        /// </summary>
        public static void Reset() {
            lock (lck) {
                contentByFile.Clear();
                frequencyByFile.Clear();
            }
        }
        
        /// <summary>
        /// Sorts frequencyByFile.
        /// DOES NOT ACQUIRE A LOCK. YOU MUST ACQUIRE A LOCK IN THE CALLER
        /// OF THIS FUNCTION.
        /// </summary>
        static void SortFrequency() {
            frequencyByFile.Sort(comparer);
        }
        
        /// <summary>
        /// Increments frequency of a file in the LFU system.
        /// DOES NOT ACQUIRE A LOCK. YOU MUST ACQUIRE A LOCK IN THE CALLER
        /// OF THIS FUNCTION.
        /// </summary>
        static void IncrementFrequency(string filePath) {
            bool found = false;
            
            foreach (FFPair pair in frequencyByFile) {
                if (pair.FilePath == filePath) {
                    pair.Frequency += 1;
                    found = true;
                    break;
                }
            }
            
            if (found) {
                SortFrequency();
            } else {
                // since frequencyByFile should be sorted in descending order
                // this should maintain order
                frequencyByFile.Add(new FFPair(filePath));
            }
        }
        
        /// <summary>
        /// Update the LFU cache after a new request for a file.
        /// Does not remove files until over size limit.
        /// </summary>
        static Task UpdateCache(string filePath = null, byte[] value = null) {
            return Task.Run(() => {
                lock (lck) {
                    // update frequency if a specific file is provided
                    if (filePath != null) {
                        IncrementFrequency(filePath);
                    }
                    
                    long bytesUsed = 0;
                    HashSet<string> pathsToKeep = new HashSet<string>();
                    bool exhausted = false;
                    
                    // go through the sorted list, starting at
                    // highest frequency
                    for (int i = 0; i < frequencyByFile.Count; i++) {
                        FFPair pair = frequencyByFile[i];
                        string path = pair.FilePath;
                        byte[] content;
                        
                        // dont waste a read if we already know the value
                        if (!contentByFile.ContainsKey(path)) {
                            // dont waste a read if the file-value pair was provided
                            if (path == filePath) {
                                content = value;
                            } else {
                                // otherwise we have to read anyway
                                content = Read(path);
                            }
                        } else {
                            content = contentByFile[path];
                        }
                        
                        // dont cache things that are over 50% of max cache size
                        if (content.Length > MAX_FILE_SIZE_IN_BYTES) {
                            continue;
                        }
                        
                        // if we would be over the maximum cache size after adding this file
                        if (bytesUsed + content.Length > MAX_CACHE_SIZE_IN_BYTES) {
                            // then we need to stop adding files and maybe remove some
                            // existing ones
                            exhausted = true;
                            break;
                        } else {
                            // if we would not be over the maximum cache size
                            
                            bytesUsed += content.Length;
                            if (!contentByFile.ContainsKey(path)) contentByFile[path] = content;
                            pathsToKeep.Add(path);
                        }
                    }
                    
                    // if we are exhausted, then we have to remove some stuff
                    if (exhausted) {
                        List<string> pathsToRemove = new List<string>();
                        
                        // if its not in paths to keep, queue it for removal
                        foreach (string path in contentByFile.Keys) {
                            if (!pathsToKeep.Contains(path)) {
                                pathsToRemove.Add(path);
                            }
                        }
                        
                        // remove everything queued for removal
                        foreach (string path in pathsToRemove) {
                            contentByFile.Remove(path);
                        }
                    }
                }
                
            });
        }
        
        /// <summary>
        /// Get the contents of a file WITHOUT caching.
        /// Returns an empty byte[] if file not found.
        /// </summary>
        public static byte[] Read(string filePath) {
            try {
                return File.ReadAllBytes(filePath);
            } catch (FileNotFoundException) {
                Log.Error("[Asypi] FileServer.Read() File not found: {0}", filePath);
                return new byte[]{};
            }
        }
        
        /// <summary>
        /// Get the contents of a file. 
        /// Returns null if file not found. LFU-cached.
        /// </summary>
        public static byte[] Get(string filePath) {
            
            lock (lck) {
                if (!contentByFile.ContainsKey(filePath)) {
                    // if we could not find file in cache
                    try {
                        byte[] content = File.ReadAllBytes(filePath);
                        
                        // this can acquire the lock later because its async
                        UpdateCache(filePath, content);
                        
                        // we have no guarantee that calling cache will actually cache it, since
                        // this is an LFU system
                        return content;
                    } catch (FileNotFoundException) {
                        Log.Error("[Asypi] FileServer.Get() File not found: {0}", filePath);
                        return null;
                    }
                } else {
                    // if we did find file in cache, re-cache it to update LFU frequency
                    // this can acquire the lock later because its async
                    UpdateCache(filePath, contentByFile[filePath]);
                    
                    return contentByFile[filePath];
                }
            }
            
        }
        
    }
}
