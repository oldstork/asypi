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
        
        /*
        TO PREVENT DEADLOCKS:
        in all cases that frequencyLock and contentLock are locked at the same time
        lock frequencyLock FIRST, and contentLock SECOND
        */
        static object contentLock = new object();
        static object frequencyLock = new object();
        
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
                    await Task.Delay(Params.FileServerEpochLength);
                    
                    lock (frequencyLock) {
                        // end of epoch, halve each frequency so that values
                        // stabilize in the long-term
                        
                        foreach (FFPair pair in frequencyByFile) {
                            pair.Frequency /= 2;
                        }
                    }
                    
                    // then update cache
                    
                    await UpdateCache();
                }
            });
            
        }
        
        /// <summary>
        /// Resets LFU cache files and frequency counters.
        /// </summary>
        public static void Reset() {
            lock (frequencyLock) {
                lock (contentLock) {
                    contentByFile.Clear();
                    frequencyByFile.Clear();
                }
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
        static void IncrementFrequencyNolock(string filePath) {
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
        /// Acquires relevant locks and
        /// increments frequency of a file in the LFU system.
        /// For async, see
        /// <see cref="IncrementFrequencyAsync" />.
        /// </summary>
        static void IncrementFrequency(string filePath) {
            lock (frequencyLock) {
                IncrementFrequencyNolock(filePath);
            }
        }
        
        /// <summary>
        /// Acquires relevant locks and
        /// increments frequency of a file in the LFU system.
        /// For sync, see
        /// <see cref="IncrementFrequency" />.
        /// </summary>
        static Task IncrementFrequencyAsync(string filePath) {
            return Task.Run(() => {
                IncrementFrequency(filePath);
            });
        }
        
        /// <summary>
        /// Update the LFU cache.
        /// Does not remove files until over size limit.
        /// </summary>
        static Task UpdateCache() {
            return Task.Run(() => {
                long bytesUsed = 0;
                HashSet<string> pathsToKeep = new HashSet<string>();
                bool exhausted = false;
                
                lock (frequencyLock) {
                    // update frequency if a specific file is provided
                    
                    lock (contentLock) {
                        // go through the sorted list, starting at
                        // highest frequency
                        for (int i = 0; i < frequencyByFile.Count; i++) {
                            FFPair pair = frequencyByFile[i];
                            string path = pair.FilePath;
                            byte[] content;
                            
                            if (!contentByFile.ContainsKey(path)) {
                                // if we don't already know the contents of the file
                                content = Read(path);
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
            
            lock (contentLock) {
                if (!contentByFile.ContainsKey(filePath)) {
                    // if we could not find file in cache
                    try {
                        byte[] content = File.ReadAllBytes(filePath);
                        
                        // update LFU frequency
                        IncrementFrequencyAsync(filePath);
                        
                        return content;
                    } catch (FileNotFoundException) {
                        Log.Error("[Asypi] FileServer.Get() File not found: {0}", filePath);
                        return null;
                    }
                } else {
                    // if we did find file in cache
                    
                    // update LFU frequency
                    IncrementFrequencyAsync(filePath);
                    
                    return contentByFile[filePath];
                }
            }
            
        }
        
    }
}
