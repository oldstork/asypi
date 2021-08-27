using System.Collections.Generic;

namespace Asypi {
    /// <summary>A utility class for adding headers to an <see cref="HttpResponse"/></summary>
    public interface IHeaders {
        /// <summary>A dictionary containing header values, indexed by header name.</summary>
        Dictionary<string, string> Values { get; }
    }
    
    /// <summary>A sensible default for headers.</summary>
    public class DefaultHeaders : IHeaders {
        public Dictionary<string, string> Values { get; private set; }
        
        public DefaultHeaders() {
            Values = new Dictionary<string, string>();
            
            Values["Server"] = "Asypi";
            Values["X-Content-Type-Options"] = "nosniff";
            Values["X-XSS-Protection"] = "1; mode=block";
            Values["X-Frame-Options"] = "SAMEORIGIN";
            Values["Content-Security-Policy"] = "script-src 'self'";
        }
    }
    
    /// <summary>A global static instance of <see cref="DefaultHeaders"/>.</summary>
    public static class DefaultHeadersInstance {
        /// <summary>The internal <see cref="DefaultHeaders"/> instance of <see cref="DefaultHeadersInstance"/>.</summary>
        public static DefaultHeaders Instance = new DefaultHeaders();
        
        // C# does not allow top-level static variable definitions, so this class is necessary.
        // DefaultHeaders is not static so that it can implement the Headers interface, and so that it is itself inheritable.
    }
}
