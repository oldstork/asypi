using System;

namespace AsypiTests {
    public delegate bool Test();
    
    public static class TestRunner {
        static int passedTests = 0;
        static int failedTests = 0;
        
        static bool didInit = false;
        
        public static void RunTest(string name, Test test) {
            if (!didInit) {
                Console.WriteLine("Running tests...\n================\n");
                didInit = true;
            }
            
            bool passed = test();
            
            ConsoleColor defaultColor = Console.ForegroundColor;
            
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(name);
            Console.ForegroundColor = defaultColor;
            Console.Write("..");
            
            if (passed) {
                passedTests += 1;
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("passed");
            } else {
                failedTests += 1;
                
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("FAILED");
            }
            
            Console.ForegroundColor = defaultColor;
            Console.Write("\n");
        }
        
        public static void PrintConclusions() {
            Console.WriteLine("");
            Console.WriteLine("----------------");
            
            ConsoleColor defaultColor = Console.ForegroundColor;
            
            Console.Write("Test Results: ");
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(passedTests.ToString());
            
            Console.ForegroundColor = defaultColor;
            Console.Write(" passing, ");
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(failedTests.ToString());
            
            Console.ForegroundColor = defaultColor;
            Console.Write(" failing\n");
            
            
            Console.Write("Build Status: ");
            
            if (failedTests > 0) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("FAILING\n");
            } else {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("passing\n");
            }
            
            Console.ForegroundColor = defaultColor;
        }
    }
}
