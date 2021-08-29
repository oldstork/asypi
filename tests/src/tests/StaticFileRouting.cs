
using Asypi;

namespace AsypiTests {
    public static class StaticFileRoutingTests {
        public static bool RouteStaticDirMatchInclude() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.RouteStaticDir("/static", "./static/", match: "^.*\\.txt$");
            
            return CliLink.CurlTest("-X GET localhost:8000/static/test.txt", "^Hello World!$");
        }
        
        public static bool RouteStaticDirMatchExclude() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.RouteStaticDir("/static", "./static/", match: "^.*\\.css$");
            
            return CliLink.CurlTest(
                "-X GET localhost:8000/static/test.txt",
                "^<!DOCTYPE html><html><body><h1>Error 404</h1><p>Resource not found</p></body></html>$"
            );
        }
    }
}
