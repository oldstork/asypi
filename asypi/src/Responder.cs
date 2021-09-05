using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Asypi {
    /// <summary>Responds to HTTP requests.</summary>
    public delegate void Responder(
        HttpRequest req,
        HttpResponse res
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
    public delegate string ComplexTextResponder(HttpRequest req, HttpResponse res);
    
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
            IHeaders headers
        ) {
            res.ContentType = contentType;
            
            IHeaders ehd = headers;
            
            if (headers == null) ehd = DefaultServerHeaders.Instance;
            
            res.LoadHeaders(ehd);
            
            res.BodyText = responseText;
        }
        
        /// <summary>
        /// Transforms the provided pseudo-responder into a proper <see cref="Responder"/>.
        /// If headers is null, will use default headers.
        /// </summary>
        static void ResponderTransformerDoc() {}
        
        /// <inheritdoc cref="ResponderTransformerDoc"/>
        public static Responder Transform(
            SimpleTextResponder simpleTextResponder,
            string contentType,
            IHeaders headers
        ) {
            return (HttpRequest req, HttpResponse res) => {
                string responseText = simpleTextResponder();
                
                SimpleResponse(req, res, responseText, contentType, headers);
            };
        }
        
        /// <inheritdoc cref="ResponderTransformerDoc"/>
        public static Responder Transform(
            ComplexTextResponder complexTextResponder,
            string contentType,
            IHeaders headers
        ) {
            return (HttpRequest req, HttpResponse res) => {
                string responseText = complexTextResponder(req, res);
                
                SimpleResponse(req, res, responseText, contentType, headers);
            };
        }
        
        
        /// <summary>The inner responder of <see cref="Respond404"/></summary>
        static Responder Respond404Text = Transform(() => {
            return "<!DOCTYPE html><html><body><h1>Error 404</h1><p>Resource not found</p></body></html>";
        }, "text/html", null);
        
        /// <summary>A sensible default 404 <see cref="Responder"/>.</summary>
        public static void Respond404(HttpRequest req, HttpResponse res) {
            res.StatusCode = (int) HttpStatusCode.NotFound;
            
            Respond404Text(req, res);
        }
    }
}
