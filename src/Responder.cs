using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Asypi {
    public delegate void Responder(HttpRequest request, HttpResponse response, List<string> args);
    
    public delegate string SimpleTextResponder();
    public delegate string ComplexTextResponder(HttpRequest request, HttpResponse response);
    public delegate string SimpleTextResponderArgs(List<string> args);
    
    static class ResponderUtils {
        static void SimpleResponse(HttpRequest req, HttpResponse res, string responseText, string contentType, Headers headers) {
            res.ContentType = contentType;
            
            Headers ehd = headers;
            
            if (headers == null) ehd = DefaultHeadersInstance.Instance;
            
            foreach (string header in ehd.Values.Keys) {
                res.Headers.Set(header, ehd.Values[header]);
            }
            
            res.Body = responseText;
        }
        
        public static Responder Transform(SimpleTextResponder simpleTextResponder, string contentType, Headers headers = null) {
            return (HttpRequest req, HttpResponse res, List<string> args) => {
                string responseText = simpleTextResponder();
                
                SimpleResponse(req, res, responseText, contentType, headers);
            };
        }
        
        public static Responder Transform(SimpleTextResponderArgs simpleTextResponderArgs, string contentType, Headers headers = null) {
            return (HttpRequest req, HttpResponse res, List<string> args) => {
                string responseText = simpleTextResponderArgs(args);
                
                SimpleResponse(req, res, responseText, contentType, headers);
            };
        }
        
        public static Responder Transform(ComplexTextResponder complexTextResponder, string contentType, Headers headers = null) {
            return (HttpRequest req, HttpResponse res, List<string> args) => {
                string responseText = complexTextResponder(req, res);
                
                SimpleResponse(req, res, responseText, contentType, headers);
            };
        }
        
        
        static Responder Respond404Text = Transform(() => {
            return "<!DOCTYPE html><html><body><h1>Error 404</h1><p>Resource not found</p></body></html>";
        }, "text/html");
        
        public static void Respond404(HttpRequest request, HttpResponse response, List<string> args) {
            response.StatusCode = 404;
            
            Respond404Text(request, response, args);
        }
    }
}
