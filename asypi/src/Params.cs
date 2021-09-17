
namespace Asypi {
    
    /// <summary>A utility class for global parameters.</summary>
    static class Params {
        /// <summary>The number of bytes in a MiB</summary>
        public const int BYTES_PER_MIB = 1024 * 1024;
        
        /// <summary>The maximum number of delimited "things" in a request path.</summary>
        public const int MAX_PATH_LENGTH = 1024;
        
        /// <summary>The number of milliseconds per <see cref="FileServer"/> epoch. Approximately 17.5 minutes.</summary>
        public static int FileServerEpochLength = 1048576;
        
        /// <summary>The maximum combined size of files cached by <see cref="FileServer"/> at a time, measured in MiB.</summary>
        public static int FileServerLFUCacheSize = 128;
    }
    
}
