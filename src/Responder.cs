using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Asypi {
    /// <summary>Responds to HTTP requests.</summary>
    public delegate void Responder(
        HttpRequest request,
        HttpResponse response,
        List<string> args
    );
    
    /// <summary>
    /// When given to a <see cref="Server"/>,
    /// is transformed into a <see cref="Responder"/>
    /// that sets the response body to the output of the function,
    /// and sets headers and content type as requested.
    /// </summary>
    delegate void TransformedResponderDoc();
    
    /// <inheritdoc cref="TransformedResponderDoc"/>
    public delegate string SimpleTextResponder();
    
    /// <inheritdoc cref="TransformedResponderDoc"/>
    public delegate string ComplexTextResponder(HttpRequest request, HttpResponse response);
    
    /// <inheritdoc cref="TransformedResponderDoc"/>
    public delegate string SimpleTextResponderArgs(List<string> args);
    
    /// <summary>A set of utilities for working with and transforming things into <see cref="Responder"/>s.</summary>
    static class ResponderUtils {
        /// <summary>
        /// Given response text, content type, headers, and a request + response,
        /// writes the provided data to the response.
        /// </summary>
        public static void SimpleResponse(
            HttpRequest req,
            HttpResponse res,
            string responseText,
            string contentType,
            Headers headers
        ) {
            res.ContentType = contentType;
            
            Headers ehd = headers;
            
            if (headers == null) ehd = DefaultHeadersInstance.Instance;
            
            res.LoadHeaders(ehd);
            
            res.Body = responseText;
        }
        
        /// <summary>Transforms the provided pseudo-responder into a proper <see cref="Responder"/>.</summary>
        static void ResponderTransformerDoc() {}
        
        /// <inheritdoc cref="ResponderTransformerDoc"/>
        public static Responder Transform(
            SimpleTextResponder simpleTextResponder,
            string contentType,
            Headers headers = null
        ) {
            return (HttpRequest req, HttpResponse res, List<string> args) => {
                string responseText = simpleTextResponder();
                
                SimpleResponse(req, res, responseText, contentType, headers);
            };
        }
        
        /// <inheritdoc cref="ResponderTransformerDoc"/>
        public static Responder Transform(
            SimpleTextResponderArgs simpleTextResponderArgs,
            string contentType,
            Headers headers = null
        ) {
            return (HttpRequest req, HttpResponse res, List<string> args) => {
                string responseText = simpleTextResponderArgs(args);
                
                SimpleResponse(req, res, responseText, contentType, headers);
            };
        }
        
        /// <inheritdoc cref="ResponderTransformerDoc"/>
        public static Responder Transform(
            ComplexTextResponder complexTextResponder,
            string contentType,
            Headers headers = null
        ) {
            return (HttpRequest req, HttpResponse res, List<string> args) => {
                string responseText = complexTextResponder(req, res);
                
                SimpleResponse(req, res, responseText, contentType, headers);
            };
        }
        
        
        /// <summary>The inner responder of <see cref="Respond404"/></summary>
        static Responder Respond404Text = Transform(() => {
            return "<!DOCTYPE html><html><body><h1>Error 404</h1><p>Resource not found</p></body></html>";
        }, "text/html");
        
        /// <summary>A sensible default 404 <see cref="Responder"/>.</summary>
        public static void Respond404(HttpRequest req, HttpResponse res, List<string> args) {
            res.StatusCode = (int) HttpStatusCode.NotFound;
            
            Respond404Text(req, res, args);
        }
    }
}
