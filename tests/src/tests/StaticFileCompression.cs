
using Asypi;

namespace AsypiTests {
    public static class StaticFileCompressionTests {
        // There is no positive test here because curl --compressed
        // seems to be incompatible with this testing setup
        
        public static bool StaticFileCompressionNegative1() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.RouteStaticFile("/compression.txt", "./static/compression.txt");
            
            return CliLink.CurlTest("-X GET localhost:8000/compression.txt", "^0000000000$");
        }
        
        public static bool StaticFileCompressionNegative2() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.CompressResponses = false;
            
            server.RouteStaticFile("/compression.txt", "./static/compression.txt");
            
            bool result = CliLink.CurlTest("-X GET localhost:8000/compression.txt", "^0000000000$");
            
            server.CompressResponses = true;
            
            return result;
        }
        
    }
}
