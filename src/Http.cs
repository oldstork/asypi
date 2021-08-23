using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.IO;

using Serilog;

namespace Asypi {
    
    public enum HttpMethod {
        Get,
        Head,
        Post,
        Put,
        Delete,
        Patch
    }
    
    public static class HttpMethodExtensions {
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
    }
    
    public class HttpRequest {
        HttpListenerRequest inner;
        
        public string[] AcceptTypes { get { return inner.AcceptTypes; } }
        public CookieCollection Cookies { get { return inner.Cookies; } }
        public bool HasEntityBody { get { return inner.HasEntityBody; } }
        public bool IsAuthenticated { get { return inner.IsAuthenticated; } }
        public bool IsLocal { get { return inner.IsLocal; } }
        public bool IsSecureConnection { get { return inner.IsSecureConnection; } }
        public IPEndPoint LocalEndPoint { get { return inner.LocalEndPoint; } }
        public Version ProtocolVersion { get { return inner.ProtocolVersion; } }
        public NameValueCollection QueryString { get { return inner.QueryString; } }
        public string RawUrl { get { return inner.RawUrl; } }
        public IPEndPoint RemoteEndPoint { get { return inner.RemoteEndPoint; } }
        public Guid RequestTraceIdentifier { get { return inner.RequestTraceIdentifier; } }
        public string ServiceName { get { return inner.ServiceName; } }
        public TransportContext TransportContext { get { return inner.TransportContext; } }
        public Uri Url { get { return inner.Url; } }
        public Uri UrlReferrer { get { return inner.UrlReferrer; } }
        public string UserAgent { get { return inner.UserAgent; } }
        public string UserHostAddress { get { return inner.UserHostAddress; } }
        public string UserHostName { get { return inner.UserHostName; } }
        public string[] UserLanguages { get { return inner.UserLanguages; } }
        
        
        string bodyText = null;
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
        
        
        public HttpRequest(HttpListenerRequest request) {
            inner = request;
        }
    }
    
    public class HttpResponse {
        HttpListenerResponse inner;
        
        public int StatusCode {
            get { return inner.StatusCode; }
            set { inner.StatusCode = value; }
        }
        
        public Encoding ContentEncoding {
            get { return inner.ContentEncoding; }
            set { inner.ContentEncoding = value; }
        }
        
        public string ContentType {
            get { return inner.ContentType; }
            set { inner.ContentType = value; }
        }
        
        public CookieCollection Cookies {
            get { return inner.Cookies; }
        }
        
        public NameValueCollection Headers {
            get { return inner.Headers; }
        }
        
        public string RedirectLocation {
            get { return inner.RedirectLocation; }
            set { inner.RedirectLocation = value; }
        }
        
        public Version ProtocolVersion {
            get { return inner.ProtocolVersion; }
        }
        
        
        bool bodySet = false;
        public string Body {
            get { return ""; }
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
        
        public HttpResponse(HttpListenerResponse response) {
            inner = response;
        }
        
        /// <summary>Closes the connection to the client without sending a response.</summary>
        public void Abort() {
            inner.Abort();
        }
    }
    
}
