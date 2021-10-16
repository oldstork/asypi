using System;

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
        
        public static bool DynamicRoutePositive() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.Route(HttpMethod.Get, "/{name}", (Req req, Res res) => { return String.Format("Hello {0}!", req.Args[0]); }, "text/plain");
            
            return CliLink.CurlTest("-X GET localhost:8000/joe", "^Hello joe!$");
        }
        
        public static bool DynamicRouteNestedPositive() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.Route(HttpMethod.Get, "/{one}/{two}", (Req req, Res res) => { return String.Format("{0} {1}!", req.Args[0], req.Args[1]); }, "text/plain");
            
            return CliLink.CurlTest("-X GET localhost:8000/foo/bar", "^foo bar!$");
        }
        
        public static bool DynamicRoutePriority() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.Route(HttpMethod.Get, "/foo", () => { return "foo"; }, "text/plain");
            server.Route(HttpMethod.Get, "/{name}", (Req req, Res res) => { return String.Format("Hello {0}!", req.Args[0]); }, "text/plain");
            
            return CliLink.CurlTest("-X GET localhost:8000/foo", "^foo$");
        }
        
    }
}
