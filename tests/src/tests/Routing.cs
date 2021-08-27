
using Asypi;

namespace AsypiTests {
    public static class RoutingTests {
        public static bool RootRoutePositive() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.Route(HttpMethod.Get, "/", () => { return "Hello World!"; }, "text/plain");
            
            return CliLink.CurlTest("-X GET localhost:8000/", "^Hello World!$");
        }
        
        public static bool RootRouteNegative() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.Route(HttpMethod.Get, "/", () => { return "Hello World!"; }, "text/plain");
            
            return CliLink.CurlTest("-X GET localhost:8000/noroute", ".*");
        }
    }
}
