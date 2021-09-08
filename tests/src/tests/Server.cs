
using Asypi;

namespace AsypiTests {
    public static class ServerTests {
        public static bool Reset() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.Route(HttpMethod.Get, "/", () => { return "Hello World!"; }, "text/plain");
            
            bool beforeReset = CliLink.CurlTest("-X GET localhost:8000/", "^Hello World!$");
            
            server.Reset();
            
            bool afterReset = CliLink.CurlTest("-X GET localhost:8000/", "^<!DOCTYPE html><html><body><h1>Error 404</h1><p>Resource not found</p></body></html>$");
            
            return beforeReset && afterReset;
        }
    }
}
