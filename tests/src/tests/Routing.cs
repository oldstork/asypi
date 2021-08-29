
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
            
            return CliLink.CurlTest("-X GET localhost:8000/noroute", "^<!DOCTYPE html><html><body><h1>Error 404</h1><p>Resource not found</p></body></html>$");
        }
    }
}
