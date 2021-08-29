using System;

using Asypi;

namespace AsypiTests {
    class Program {
        public static Server Server = new Server(logLevel: LogLevel.Fatal);
        
        static void Main(string[] args) {
            Server.RunAsync();
            
            TestRunner.RunTest("Root Route Positive", RoutingTests.RootRoutePositive);
            TestRunner.RunTest("Root Route Negative", RoutingTests.RootRouteNegative);
            
            TestRunner.RunTest("Route Static Dir Match Include", StaticFileRoutingTests.RouteStaticDirMatchInclude);
            TestRunner.RunTest("Route Static Dir Match Exclude", StaticFileRoutingTests.RouteStaticDirMatchExclude);
            
            TestRunner.PrintConclusions();
        }
    }
}
