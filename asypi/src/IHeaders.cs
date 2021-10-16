using System.Collections.Generic;

namespace Asypi {
    /// <summary>A utility class for adding headers to an <see cref="Res"/></summary>
    public interface IHeaders {
        /// <summary>A dictionary containing header values, indexed by header name.</summary>
        Dictionary<string, string> Values { get; }
    }
    
    
    // DefaultHeaders is not static so that it can implement the Headers interface, and so that it is itself inheritable.
    
    /// <summary>A sensible default for headers.</summary>
    public class DefaultHeaders : IHeaders {
        public Dictionary<string, string> Values { get; private set; }
        
        public DefaultHeaders() {
            Values = new Dictionary<string, string>();
            
            Values["Server"] = "Asypi";
            Values["X-Content-Type-Options"] = "nosniff";
            Values["X-XSS-Protection"] = "1; mode=block";
            Values["X-Frame-Options"] = "SAMEORIGIN";
            Values["Content-Security-Policy"] = "script-src 'self'; object-src 'none'; require-trusted-types-for 'script'";
        }
    }
    
    /// <summary>The <see cref="IHeaders" /> instance used by the <see cref="Server" /> for special responders (e.g. <see cref="Server.RouteStaticFile(string, string, string)" />) by default.</summary>
    public static class DefaultServerHeaders {
        /// <summary>The internal instance of <see cref="DefaultServerHeaders"/>.</summary>
        public static IHeaders Instance = new DefaultHeaders();
        
        // C# does not allow top-level static variable definitions, so this class is necessary.
    }
}
