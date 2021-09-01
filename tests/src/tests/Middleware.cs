
using Asypi;

namespace AsypiTests {
    public static class MiddlewareTests {
        public static bool MiddlewareReturnTrue() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.Use(".*", (HttpRequest req, HttpResponse res) => {
                return true;
            });
            
            server.Route(HttpMethod.Get, "/", () => { return "Hello World!"; }, "text/plain");
            
            return CliLink.CurlTest("-X GET localhost:8000/", "^Hello World!$");
        }
        
        public static bool MiddlewareReturnFalse() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.Use(".*", (HttpRequest req, HttpResponse res) => {
                return false;
            });
            
            server.Route(HttpMethod.Get, "/", () => { return "Hello World!"; }, "text/plain");
            
            return CliLink.CurlTest("-X GET localhost:8000/", "^$");
        }
        
        public static bool MiddlewareSetResponse() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.Use(".*", (HttpRequest req, HttpResponse res) => {
                res.BodyText = "foo";
                return false;
            });
            
            server.Route(HttpMethod.Get, "/", () => { return "Hello World!"; }, "text/plain");
            
            return CliLink.CurlTest("-X GET localhost:8000/", "^foo$");
        }
    }
}