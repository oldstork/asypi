using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Asypi {
    public static class Validation {
        const string PATH_REGEX_EXPRESSION = "(?<=:.*:.*/).*";
        public static Regex PathRegex { get; private set; }
        
        static HashSet<char> VALID_LOWERCASE_HOSTNAME_CHARS = new HashSet<char>() { 
            'a',
            'b',
            'c',
            'd',
            'e',
            'f',
            'g',
            'h',
            'i',
            'j',
            'k',
            'l',
            'm',
            'n',
            'o',
            'p',
            'q',
            'r',
            's',
            't',
            'u',
            'v',
            'w',
            'x',
            'y',
            'z',
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            '-',
            '_',
            '.',
            '*'
        };
        
        static HashSet<char> VALID_LOWERCASE_SUBPATH_CHARS = new HashSet<char>() { 
            'a',
            'b',
            'c',
            'd',
            'e',
            'f',
            'g',
            'h',
            'i',
            'j',
            'k',
            'l',
            'm',
            'n',
            'o',
            'p',
            'q',
            'r',
            's',
            't',
            'u',
            'v',
            'w',
            'x',
            'y',
            'z',
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            '-',
            '_',
            '{',
            '}'
        };
        
        
        static Validation() {
            PathRegex = new Regex(PATH_REGEX_EXPRESSION, RegexOptions.Compiled);
        }
        
        static bool ValidateAgainstWhitelist(string str, HashSet<char> whitelist) {
            string strLower = str.ToLower();
            
            foreach (char c in strLower.ToCharArray()) {
                if (!whitelist.Contains(c)) {
                    return false;
                }
            }
            
            return true;
        }
        
        public static bool IsHostnameValid(string hostname) {
            return ValidateAgainstWhitelist(hostname, VALID_LOWERCASE_HOSTNAME_CHARS);
        }
        
        public static bool IsSubPathValid(string subpath) {
            return ValidateAgainstWhitelist(subpath, VALID_LOWERCASE_SUBPATH_CHARS);
        }
    }
}
