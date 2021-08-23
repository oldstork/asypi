using System;
using System.Collections.Generic;
using System.Linq;


namespace Asypi {
    public static class Utils {
        public static List<T> PossibleValues<T>() where T: Enum {
            return new List<T>( (T[])  Enum.GetValues(typeof(T)) );
        }
        
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
    }
}
