using System;
using System.Collections.Generic;
using System.Linq;


namespace Asypi {
    /// <summary>Basic miscellaneous internal utils.</summary>
    static class Utils {
        /// <summary>Get a list of all possible values of an <see cref="Enum"/>.</summary>
        public static List<T> PossibleValues<T>() where T: Enum {
            return new List<T>( (T[])  Enum.GetValues(typeof(T)) );
        }
        
        /// <summary>Split a request path into its components.</summary>
        public static List<string> SplitPath(string path) {
            string[] initial = path.Split('/');
            
            List<string> splitPath = new List<string>();
            
            foreach (string str in initial) {
                if (str.Length > 0 && Validation.IsSubPathValid(str)) {
                    splitPath.Add(str);
                }
            }
            
            return splitPath;
        }
        
        /// <summary>Split a file path into its components. Works regardless of path delimiter.</summary>
        public static string[] SplitFilePath(string path) {
            if (path.Contains('\\')) {
                return path.Split('\\');
            } else {
                return path.Split('/');
            }
        }
    }
}
