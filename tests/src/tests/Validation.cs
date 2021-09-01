
using Asypi;

namespace AsypiTests {
    public static class ValidationTests {
        public static bool PrecedingSlashInsertion() {
            Server server = Program.Server;
            
            server.Reset();
            
            server.Route(HttpMethod.Get, "foo", () => { return "Hello World!"; }, "text/plain");
            
            return CliLink.CurlTest("-X GET localhost:8000/foo", "^Hello World!$");
        }
    }
}