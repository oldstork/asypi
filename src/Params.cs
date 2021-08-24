
namespace Asypi {
    
    /// <summary>A utility class for hardcoded values.</summary>
    static class Params {
        /// <summary>The maximum number of delimited "things" in a request path.</summary>
        public const int MAX_PATH_LENGTH = 1024;
        
        /// <summary>The number of files cached by <see cref="FileServer"/> at a time.</summary>
        public const int FILESERVER_LRU_CACHE_SIZE = 32;
    }
    
}