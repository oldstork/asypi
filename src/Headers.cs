using System.Collections.Generic;

namespace Asypi {
    public interface Headers {
        Dictionary<string, string> Values { get; }
    }
    
    public class DefaultHeaders : Headers {
        protected Dictionary<string, string> values = new Dictionary<string, string>();
        public Dictionary<string, string> Values { get { return values; } }
        
        public DefaultHeaders() {
            values["Server"] = "Asypi";
            values["X-Content-Type-Options"] = "nosniff";
            values["X-XSS-Protection"] = "1; mode=block";
            values["X-Frame-Options"] = "SAMEORIGIN";
            values["Content-Security-Policy"] = "script-src 'self'";
        }
    }
    
    public static class DefaultHeadersInstance {
        public static DefaultHeaders Instance = new DefaultHeaders();
    }
}
