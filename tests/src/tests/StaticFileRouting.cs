
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
        
        public static bool RouteStaticDirDepthInclude() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.RouteStaticDir("/static", "./static/", maxDepth: 2);
            
            return CliLink.CurlTest(
                "-X GET localhost:8000/static/foo/alfa.txt",
                "^Alfa$"
            );
        }
        
        public static bool RouteStaticDirDepthExclude() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.RouteStaticDir("/static", "./static/", maxDepth: 2);
            
            return CliLink.CurlTest(
                "-X GET localhost:8000/static/foo/bar/bravo.txt",
                "^<!DOCTYPE html><html><body><h1>Error 404</h1><p>Resource not found</p></body></html>$"
            );
        }
    }
}
