using System.Collections.Generic;
using System.IO;

using Serilog;

namespace Asypi {
    
    /// <summary>A utility class for storing and serving files. Minimizes disk reads via an LRU cache.</summary>
    public static class FileServer {
        // LRU cache
        static Dictionary<string, byte[]> contentByFile = new Dictionary<string, byte[]>();
        static string[] recency;
        static int start = 0;
        static int end;
        
        
        public static void Init() {
            recency = new string[Params.FileServerLRUCacheSize];
            end = Params.FileServerLRUCacheSize - 1;
        }
        
        static void AdvanceLRUIndices() {
            // LRU indices should be advanced by subtraction,
            // so that as they are advanced a given index gets closer to end
            // and farther from start
            
            start -= 1;
            if (start < 0) start = Params.FileServerLRUCacheSize - 1;
            
            end -= 1;
            if (end < 0) end = Params.FileServerLRUCacheSize - 1;
        }
        
        /// <summary>Add a file-value pair to the LRU cache, and update the cache as necessary.</summary>
        static void Cache(string filePath, byte[] value) {
            for (int i = 0; i < Params.FileServerLRUCacheSize; i++) {
                if (recency[i] == filePath) {
                    // if we found filePath in the cache
                    // swap it with the first item in the cache
                    // this should not result in a performance slowdown
                    // except in rare cases, but the performance benefit
                    // from less expensive recency updates should be worth it
                    if (i != start) {
                        string tmp = recency[start];
                        
                        recency[start] = recency[i];
                        recency[i] = tmp;
                    }
                    
                    // if we found it in the cache, return so we dont add things to cache
                    return;
                }
            }
            
            // if we did not find filePath in the cache
            // uncache the least recent file
            if (recency[end] != null) contentByFile.Remove(recency[end]);
            
            AdvanceLRUIndices();
            
            // start is what used to be end
            recency[start] = filePath;
            contentByFile[filePath] = value;
        }
        
        /// <summary>
        /// Get the contents of a file WITHOUT caching.
        /// Returns an empty string if file not found.
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
        /// Returns null if file not found. LRU-cached.
        /// </summary>
        public static byte[] Get(string filePath) {
            if (!contentByFile.ContainsKey(filePath)) {
                // if we could not find file in cache
                try {
                    Cache(filePath, File.ReadAllBytes(filePath));
                } catch (FileNotFoundException) {
                    Log.Error("[Asypi] FileServer.Get() File not found: {0}", filePath);
                    return null;
                }
            } else {
                // if we did find file in cache, re-cache it to reset LRU recency
                Cache(filePath, contentByFile[filePath]);
            }
            
            return contentByFile[filePath];
        }
    }
}
