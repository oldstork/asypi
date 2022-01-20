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
    
    /// <summary>
    /// <see cref="DefaultHeaders" /> with a 
    /// <c>Set()</c> method to easily add additional headers.
    /// </summary>
    public class DefaultHeadersMut : DefaultHeaders {
        public DefaultHeadersMut() : base() {}
        
        public void Set(string key, string value) {
            Values[key] = value;
        }
    }
    
    /// <summary><see cref="DefaultHeadersMut" />, with a cache control header.</summary>
    public class DefaultStaticFileHeaders : DefaultHeadersMut {
        public DefaultStaticFileHeaders() : base() {
            Values["Cache-Control"] = "public, max-age=86400";
        }
    }
    
    /// <summary>
    /// The <see cref="IHeaders" /> instances used by the 
    /// <see cref="Server" /> for special responders 
    /// (e.g. <see cref="Server.RouteStaticFile(string, string, string)" />) by default.
    /// </summary>
    public static class DefaultServerHeaders {
        /// <summary>The internal instance of <see cref="DefaultServerHeaders"/>.</summary>
        public static DefaultHeaders DefaultHeadersInstance = new DefaultHeaders();
        
        /// <summary>The internal instance of <see cref="DefaultStaticFileHeaders"/>.</summary>
        public static DefaultStaticFileHeaders DefaultStaticFileHeadersInstance = new DefaultStaticFileHeaders();
        
        // C# does not allow top-level static variable definitions, so this class is necessary.
    }
}
