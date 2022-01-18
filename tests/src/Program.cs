using System;
using System.Threading;

using Asypi;

namespace AsypiTests {
    class Program {
        public static Server Server = new Server(logLevel: LogLevel.Fatal);
        public static bool Verbose = false;
        
        static void Main(string[] args) {
            if (args.Length > 0) {
                string arg1 = args[0].ToLower();
                if (arg1 == "verbose" || arg1 == "--verbose") {
                    Verbose = true;
                }
            }
            
            
            Server.RunAsync();
            
            // Ensure server is fully initialized
            Thread.Sleep(50);
            
            TestRunner.RunTest("Server Reset", ServerTests.Reset);
            
            TestRunner.RunTest("Root Route Positive", RoutingTests.RootRoutePositive);
            TestRunner.RunTest("Root Route Negative", RoutingTests.RootRouteNegative);
            TestRunner.RunTest("Dynamic Route Positive", RoutingTests.DynamicRoutePositive);
            TestRunner.RunTest("Dynamic Route Nested Positive", RoutingTests.DynamicRouteNestedPositive);
            TestRunner.RunTest("Dynamic Route Priority", RoutingTests.DynamicRoutePriority);
            
            TestRunner.RunTest("Route Static Dir Match Include", StaticFileRoutingTests.RouteStaticDirMatchInclude);
            TestRunner.RunTest("Route Static Dir Match Exclude", StaticFileRoutingTests.RouteStaticDirMatchExclude);
            TestRunner.RunTest("Route Static Dir Depth Include", StaticFileRoutingTests.RouteStaticDirDepthInclude);
            TestRunner.RunTest("Route Static Dir Depth Exclude", StaticFileRoutingTests.RouteStaticDirDepthExclude);
            
            TestRunner.RunTest("Static File Compression Negative 1", StaticFileCompressionTests.StaticFileCompressionNegative1);
            TestRunner.RunTest("Static File Compression Negative 2", StaticFileCompressionTests.StaticFileCompressionNegative2);
            
            TestRunner.RunTest("FileServer Get File", FileServerTests.GetFile);
            TestRunner.RunTest("FileServer Get File Cached", FileServerTests.GetFileCached);
            
            TestRunner.RunTest("Middleware Return True", MiddlewareTests.MiddlewareReturnTrue);
            TestRunner.RunTest("Middleware Return False", MiddlewareTests.MiddlewareReturnFalse);
            TestRunner.RunTest("Middleware Set Response", MiddlewareTests.MiddlewareSetResponse);
            
            TestRunner.RunTest("Preceding Slash Insertion", ValidationTests.PrecedingSlashInsertion);
            
            TestRunner.PrintConclusions();
        }
    }
}
