
namespace Asypi {
    
    /// <summary>A utility class for global parameters.</summary>
    static class Params {
        /// <summary>The current version string.</summary>
        public const string ASYPI_VERSION = "0.1.1";
        
        /// <summary>The number of bytes in a MiB.</summary>
        public const int BYTES_PER_MIB = 1024 * 1024;
        
        /// <summary>The maximum number of delimited "things" in a request path.</summary>
        public const int MAX_PATH_LENGTH = 1024;
        
        /// <summary>The default value for the number of milliseconds per <see cref="FileServer"/> epoch. Approximately 17.5 minutes.</summary>
        public const int DEFAULT_FILESERVER_EPOCH_LENGTH = 1048576;
        
        /// <summary>The default value for the maximum combined size of files cached by <see cref="FileServer"/> at a time, measured in MiB.</summary>
        public const int DEFAULT_FILESERVER_LFU_CACHE_SIZE = 128;
    }
    
}
