using System;
using System.Text;

using Asypi;

namespace AsypiTests {
    public static class FileServerTests {
        public static bool GetFile() {
            byte[] content = FileServer.Get("./static/test.txt");
            
            return (Encoding.UTF8.GetString(content) == "Hello World!");
        }
        
        public static bool GetFileCached() {
            byte[] content = FileServer.Get("./static/test.txt");
            // try another file to make sure cache retains
            content = FileServer.Get("./static/test.css");
            content = FileServer.Get("./static/test.txt");
            
            return (Encoding.UTF8.GetString(content) == "Hello World!");
        }
    }
}
