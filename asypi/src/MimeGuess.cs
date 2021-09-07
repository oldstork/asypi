using System.Text.RegularExpressions;

namespace Asypi {
    /// <summary>A utility class for guessing mime types of files and text.</summary>
    static class MimeGuesser {
        /// <summary>Given a file path, attempts to guess an appropriate mime type.</summary>
        public static string GuessTypeByExtension(string filePath) {
            // grab the file extension
            Match extensionMatch = Validation.FileExtensionRegex.Match(filePath);
            
            // try to guess by file extension
            if (extensionMatch.Success) {
                switch (extensionMatch.ToString()) {
                    case ".aac":
                        return "audio/aac";
                    case ".css":
                        return "text/css";
                    case ".gif":
                        return "image/gif";
                    case ".htm":
                        return "text/html";
                    case ".html":
                        return "text/html";
                    case ".ico":
                        return "image/vnd.microsoft.icon";
                    case ".jpeg":
                        return "image/jpeg";
                    case ".jpg":
                        return "image/jpeg";
                    case ".js":
                        return "text/javascript";
                    case ".json":
                        return "application/json";
                    case ".mov":
                        return "video/quicktime";
                    case ".mp3":
                        return "audio/mpeg";
                    case ".mp4":
                        return "video/mp4";
                    case ".mpeg":
                        return "video/mpeg";
                    case ".png":
                        return "image/png";
                    case ".pdf":
                        return "application/pdf";
                    case ".svg":
                        return "image/svg+xml";
                    case ".ttf":
                        return "font/ttf";
                    case ".txt":
                        return "text/plain";
                    case ".weba":
                        return "audio/webm";
                    case ".webm":
                        return "video/webm";
                    case ".webp":
                        return "image/webp";
                    case ".xml":
                        return "application/xml";
                    case ".zip":
                        return "application/zip";
                }
            }
            
            return "text/plain";
        }
    }
    
}