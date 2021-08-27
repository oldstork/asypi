using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AsypiTests {
    public static class CliLink {
        public static bool CurlTest(string args, string regexPattern) {
            Process process = new Process();
            process.StartInfo.FileName = "curl";
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            
            
            process.Start();
            
            process.WaitForExit();
            
            
            string text = "";
            
            // this hack needs to exist because we have
            // no guarantee of an end of stream character
            Task.Run(() => {
                while (!process.StandardOutput.EndOfStream) {
                    text += process.StandardOutput.ReadLine();
                }
            });
            
            // wait for read to complete
            Thread.Sleep(32);
            
            Regex regex = new Regex(regexPattern);
            return regex.Match(text).Success;
        }
    }
}
