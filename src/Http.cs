using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.IO;

using Serilog;

namespace Asypi {
    
    /// <summary>A type-safe representation of a limited subset of HTTP methods.</summary>
    public enum HttpMethod {
        Get,
        Head,
        Post,
        Put,
        Delete,
        Patch
    }
    
    public static class HttpMethodExtensions {
        /// <summary>Converts a string representing an HTTP method into an <see cref="HttpMethod"/>.</summary>
        public static HttpMethod? ToHttpMethod(this string httpMethodString) {
            switch (httpMethodString) {
                case "GET":
                    return HttpMethod.Get;
                case "HEAD":
                    return HttpMethod.Head;
                case "POST":
                    return HttpMethod.Post;
                case "PUT":
                    return HttpMethod.Put;
                case "DELETE":
                    return HttpMethod.Delete;
                case "PATCH":
                    return HttpMethod.Patch;
            }
            
            return null;
        }
        
        /// <summary>Converts an <see cref="HttpMethod"/> into a string representing an HTTP method.</summary>
        public static string AsString(this HttpMethod method) {
            switch (method) {
                case HttpMethod.Get:
                    return "GET";
                case HttpMethod.Head:
                    return "HEAD";
                case HttpMethod.Post:
                    return "POST";
                case HttpMethod.Put:
                    return "PUT";
                case HttpMethod.Delete:
                    return "DELETE";
                case HttpMethod.Patch:
                    return "PATCH";
            }
            
            return "UNKNOWN";
        }
    }
    
    /// <summary>A wrapper over <see cref="HttpListenerRequest"/>. Includes important information about an HTTP request.</summary>
    public class HttpRequest {
        // the wrapped HttpListenerRequest
        HttpListenerRequest inner;
        
        // most documentation below directly mirrors that of the field wrapped
        // unfortunately inheritdoc doesn't seem to work here
        
        /// <summary>The MIME types accepted by the client.</summary>
        /// <returns>A <see cref="string"/> array that contains the type names specified in the request's Accept header or <c>null</c> if the client request did not include an Accept header.</returns>
        public string[] AcceptTypes { get { return inner.AcceptTypes; } }
        
        /// <summary>Gets the cookies sent with the request.</summary>
        /// <returns>A <see cref="CookieCollection"/> that contains cookies that accompany the request. This property returns an empty collection if the request does not contain cookies.</returns>
        public CookieCollection Cookies { get { return inner.Cookies; } }
        
        /// <summary>Gets a <see cref="bool"/> value that indicates whether the request has associated body data.</summary>
        /// <returns><c>true</c> if the request has associated body data; otherwise, <c>false</c>.</returns>
        public bool HasEntityBody { get { return inner.HasEntityBody; } }
        
        /// <summary>Gets a <see cref="bool"/> value that indicates whether the client sending this request is authenticated.</summary>
        /// <returns><c>true</c> if the request has associated body data; otherwise, <c>false</c>.</returns>
        public bool IsAuthenticated { get { return inner.IsAuthenticated; } }
        
        /// <summary>Gets a <see cref="bool"/> value that indicates whether the request is sent from the local computer.</summary>
        /// <returns><c>true</c> if the request originated on the same computer as the <see cref="HttpListener"/> object that provided the request; otherwise, <c>false</c>.</returns>
        public bool IsLocal { get { return inner.IsLocal; } }
        
        /// <summary>Gets a <see cref="bool"/> value that indicates whether the TCP connection used to send the request is using the Secure Sockets Layer (SSL) protocol.</summary>
        /// <returns><c>true</c> if the TCP connection is using SSL; otherwise, <c>false</c>.</returns>
        public bool IsSecureConnection { get { return inner.IsSecureConnection; } }
        
        /// <summary>Gets the server IP address and port number to which the request is directed.</summary>
        /// <returns>An <see cref="IPEndPoint"/> that represents the IP address that the request is sent to.</returns>
        public IPEndPoint LocalEndPoint { get { return inner.LocalEndPoint; } }
        
        /// <summary>Gets the HTTP version used by the requesting client.</summary>
        /// <returns>A <see cref="Version"/> that identifies the client's version of HTTP.</returns>
        public Version ProtocolVersion { get { return inner.ProtocolVersion; } }
        
        /// <summary>Gets the query string included in the request.</summary>
        /// <returns>A <see cref="NameValueCollection"/> object that contains the query data included in the request <see cref="HttpListenerRequest.Url"/>.</returns>
        public NameValueCollection QueryString { get { return inner.QueryString; } }
        
        /// <summary>Gets the URL information (without the host and port) requested by the client.</summary>
        /// <returns>A <see cref="string"/> that contains the raw URL for this request.</returns>
        public string RawUrl { get { return inner.RawUrl; } }
        
        /// <summary>Gets the client IP address and port number from which the request originated.</summary>
        /// <returns>An <see cref="IPEndPoint"/> that represents the IP address and port number from which the request originated.</returns>
        public IPEndPoint RemoteEndPoint { get { return inner.RemoteEndPoint; } }
        
        /// <summary>Gets the request identifier of the incoming HTTP request.</summary>
        /// <returns>A <see cref="Guid"/> object that contains the identifier of the HTTP request.</returns>
        public Guid RequestTraceIdentifier { get { return inner.RequestTraceIdentifier; } }
        
        /// <summary>Gets the Service Provider Name (SPN) that the client sent on the request.</summary>
        /// <returns>A <see cref="string"/> that contains the SPN the client sent on the request.</returns>
        public string ServiceName { get { return inner.ServiceName; } }
        
        /// <summary>Gets the <see cref="TransportContext"/> for the client request.</summary>
        /// <returns>A <see cref="TransportContext"/> object for the client request.</returns>
        public TransportContext TransportContext { get { return inner.TransportContext; } }
        
        /// <summary>Gets the <see cref="Uri"/> object requested by the client.</summary>
        /// <returns>A <see cref="Uri"/> object that identifies the resource requested by the client.</returns>
        public Uri Url { get { return inner.Url; } }
        
        /// <summary>Gets the Uniform Resource Identifier (URI) of the resource that referred the client to the server.</summary>
        /// <returns>A <see cref="Uri"/> object that contains the text of the request's <see cref="HttpRequestHeader.Referer"/> header, or <c>null</c> if the header was not included in the request.</returns>
        public Uri UrlReferrer { get { return inner.UrlReferrer; } }
        
        /// <summary>Gets the user agent presented by the client.</summary>
        /// <returns>A <see cref="string"/> object that contains the text of the request's <c>User-Agent</c> header.</returns>
        public string UserAgent { get { return inner.UserAgent; } }
        
        /// <summary>Gets the server IP address and port number to which the request is directed.</summary>
        /// <returns>A <see cref="string"/> that contains the host address information.</returns>
        public string UserHostAddress { get { return inner.UserHostAddress; } }
        
        /// <summary>Gets the DNS name and, if provided, the port number specified by the client.</summary>
        /// <returns>A <see cref="string"/> value that contains the text of the request's <c>Host</c> header.</returns>
        public string UserHostName { get { return inner.UserHostName; } }
        
        /// <summary>Gets the natural languages that are preferred for the response.</summary>
        /// <returns>A <see cref="string"/> array that contains the languages specified in the request's <see cref="HttpRequestHeader.AcceptLanguage"/> header or <c>null</c> if the client request did not include an <see cref="HttpRequestHeader.AcceptLanguage"/> header.</returns>
        public string[] UserLanguages { get { return inner.UserLanguages; } }
        
        
        // cached body text
        string bodyText = null;
        /// <summary>The body text sent with the request.</summary>
        public string Body {
            get {
                if (bodyText == null) {
                    using (var reader = new StreamReader(inner.InputStream, Encoding.UTF8)) {
                        bodyText = reader.ReadToEnd();
                    }
                }
                
                return bodyText;
            }
        }
        
        /// <summary>Creates a new <see cref="HttpRequest"/> wrapping the provided <see cref="HttpListenerRequest"/>.</summary>
        public HttpRequest(HttpListenerRequest request) {
            inner = request;
        }
    }
    
    
    /// <summary>A wrapper over <see cref="HttpListenerResponse"/>. Allows reading and setting important fields of an HTTP response.</summary>
    public class HttpResponse {
        // the wrapped HttpListenerResponse
        HttpListenerResponse inner;
        
        // most documentation below directly mirrors that of the field wrapped
        // unfortunately inheritdoc doesn't seem to work here
        
        /// <summary>Gets or sets the HTTP status code to be returned to the client.</summary>
        /// <returns>An <see cref="int"/> value that specifies the HTTP status code for the requested resource. The default is <see cref="HttpStatusCode.OK"/>, indicating that the server successfully processed the client's request and included the requested resource in the response body.</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ProtocolViolationException"></exception>
        public int StatusCode {
            get { return inner.StatusCode; }
            set { inner.StatusCode = value; }
        }
        
        /// <summary>Gets or sets the <see cref="Encoding"/> for this response's <see cref="HttpListenerResponse.OutputStream"/>.</summary>
        /// <returns>An <see cref="Encoding"/> object suitable for use with the data in the <see cref="HttpListenerResponse.OutputStream"/> property, or <c>null</c> if no encoding is specified.</returns>
        public Encoding ContentEncoding {
            get { return inner.ContentEncoding; }
            set { inner.ContentEncoding = value; }
        }
        
        /// <summary>Gets or sets the MIME type of the content returned.</summary>
        /// <returns>A <see cref="string"/> instance that contains the text of the response's <c>Content-Type</c> header.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public string ContentType {
            get { return inner.ContentType; }
            set { inner.ContentType = value; }
        }
        
        /// <summary>Gets or sets the collection of cookies returned with the response.</summary>
        /// <returns>A <see cref="CookieCollection"/> that contains cookies to accompany the response. The collection is empty if no cookies have been added to the response.</returns>
        public CookieCollection Cookies {
            get { return inner.Cookies; }
        }
        
        /// <summary>Gets or sets the collection of header name/value pairs returned by the server.</summary>
        /// <returns>A <see cref="WebHeaderCollection "/> instance that contains all the explicitly set HTTP headers to be included in the response.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public NameValueCollection Headers {
            get { return inner.Headers; }
        }
        
        /// <summary>Gets or sets the value of the HTTP <c>Location</c> header in this response.</summary>
        /// <returns>A <see cref="string"/> that contains the absolute URL to be sent to the client in the <c>Location</c> header.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public string RedirectLocation {
            get { return inner.RedirectLocation; }
            set { inner.RedirectLocation = value; }
        }
        
        /// <summary>Gets the HTTP version used for the response.</summary>
        /// <returns>A <see cref="Version"/> object indicating the version of HTTP used when responding to the client.</returns>
        public Version ProtocolVersion {
            get { return inner.ProtocolVersion; }
        }
        
        
        // whether or not the body has already been set
        bool bodySet = false;
        
        /// <summary>
        /// The body text that will be sent with the response.
        /// <br />
        /// Due to quirks related to <see cref="HttpListenerResponse.OutputStream"/>,
        /// this value should only be set once per <see cref="HttpResponse"/>.
        /// Attempting to set it multiple times will result in no changes and
        /// a logged warning.
        /// </summary>
        public string Body {
            set {
                if (bodySet) {
                    Log.Warning("[Asypi] Attempted to set body twice on response");
                } else {
                    bodySet = true;
                    
                    byte[] buffer = Encoding.UTF8.GetBytes(value);
                    
                    inner.ContentLength64 = buffer.Length;
                    
                    Stream output = inner.OutputStream;
                    
                    output.Write(buffer, 0, buffer.Length);
                }
            }
        }
        
        public byte[] BodyBytes {
            set {
                if (bodySet) {
                    Log.Warning("[Asypi] Attempted to set body twice on response");
                } else {
                    bodySet = true;
                    
                    byte[] buffer = value;
                    
                    inner.ContentLength64 = buffer.Length;
                    
                    Stream output = inner.OutputStream;
                    
                    output.Write(buffer, 0, buffer.Length);
                }
            }
        }
        
        /// <summary>Creates a new <see cref="HttpResponse"/> wrapping the provided <see cref="HttpListenerResponse"/>.</summary>
        public HttpResponse(HttpListenerResponse response) {
            inner = response;
        }
        
        /// <summary>Loads headers from the provided <see cref="Asypi.Headers"/> and sets the headers of the response to the loaded values.</summary>
        public void LoadHeaders(Headers headers) {
            foreach (string header in headers.Values.Keys) {
                Headers.Set(header, headers.Values[header]);
            }
        }
        
        /// <summary>Closes the connection to the client without sending a response.</summary>
        public void Abort() {
            inner.Abort();
        }
    }
    
}
