using System;
using System.Collections.Generic;

using Serilog;

namespace Asypi {
    /// <summary>Basic miscellaneous internal utils.</summary>
    static class Utils {
        /// <summary>Get a list of all possible values of an <see cref="Enum"/>.</summary>
        public static List<T> PossibleValues<T>() where T: Enum {
            return new List<T>( (T[])  Enum.GetValues(typeof(T)) );
        }
        
        /// <summary>
        /// Split a request path into its components.
        /// If path is internal (not from an outside request),
        /// will log warnings if issues occur.
        /// </summary>
        public static List<string> SplitPath(string path, bool isInternal = false) {
            List<string> splitPath = new List<string>();
            
            // deal with a few edge cases
            if (path.Length <= 1) {
                return splitPath;
            }
            
            string[] initial = path.Split('/');
            
            foreach (string str in initial) {
                // we're going to get some 0 length sub strings
                // because of the preceding slash
                if (str.Length > 0) {
                    if (Validation.IsSubPathValid(str)) {
                        splitPath.Add(str);
                    } else {
                        Log.Warning("[Asypi] Could not successfully split path {0} into subpaths", path);
                    }
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
