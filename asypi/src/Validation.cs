using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Asypi {
    /// <summary>A utility class for validating things.</summary>
    static class Validation {
        const string PATH_REGEX_EXPRESSION = "(?<=:.*:.*/).*";
        /// <summary>Grabs the requested resource path from a URL, if possible.</summary>
        public static Regex PathRegex { get; private set; }
        
        const string FILE_EXTENSION_REGEX = "\\.[0-9a-z]+$";
        /// <summary>Grabs a file extension from a path, if one exists.</summary>
        public static Regex FileExtensionRegex { get; private set; }
        
        const string HOSTNAME_REGEX = "^[a-zA-Z0-9-_.*]*$";
        /// <summary>Matches if and only if input text is a valid hostname.</summary>
        public static Regex HostnameRegex { get; private set; }
        
        const string SUBPATH_REGEX = "^[a-zA-Z0-9-_{}]*$";
        /// <summary>Matches if and only if input text is a valid subpath.</summary>
        public static Regex SubPathRegex { get; private set; }
        
        
        static Validation() {
            PathRegex = new Regex(PATH_REGEX_EXPRESSION, RegexOptions.Compiled);
            FileExtensionRegex = new Regex(FILE_EXTENSION_REGEX, RegexOptions.Compiled);
            HostnameRegex = new Regex(HOSTNAME_REGEX, RegexOptions.Compiled);
            SubPathRegex = new Regex(SUBPATH_REGEX, RegexOptions.Compiled);
        }
        
        /// <summary>Returns true if the given string is a valid hostname, and false otherwise.</summary>
        public static bool IsHostnameValid(string hostname) {
            return HostnameRegex.Match(hostname).Success;
        }
        
        /// <summary>Returns true if the given string is a valid subpath, and false otherwise.</summary>
        public static bool IsSubPathValid(string subpath) {
            return SubPathRegex.Match(subpath).Success;
        }
    }
}
