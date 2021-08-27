using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

using Serilog;

namespace Asypi {
    
    /* 
        Currently does nothing that couldn't be done in an async lambda under Server, BUT
        in the future, if we ever want finer control over workers (we probably will),
        this will become useful
    */
    
    /// <summary>A worker for a <see cref="Asypi.Server"/>.</summary>
    class Worker {
        /// <summary>
        /// The <see cref="Asypi.Server"/>
        /// that this <see cref="Worker"/> is attached to.
        /// </summary>
        public Server Server { get; private set; }
        
        /// <summary>
        /// The <see cref="Router"/> of the <see cref="Asypi.Server"/>
        /// that this <see cref="Worker"/> is attached to.
        /// </summary>
        Router router;
        
        /// <summary>
        /// Creates a new worker attached to the provided
        /// <see cref="Asypi.Server"/> and <see cref="Router"/>.
        /// </summary>
        public Worker(Server server, Router router) {
            this.Server = server;
            this.router = router;
        }
        
        /// <summary>
        /// Runs the <see cref="Worker"/>.
        /// </summary>
        public Task Run(HttpListenerContext context) {
            return Task.Run(() => {
                // start diagnostics
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                
                // grab httplistenerrequest/response from context
                HttpListenerRequest httpRequest = context.Request;
                HttpListenerResponse httpResponse = context.Response;
                
                // create user facing req and res objects
                HttpRequest req = new HttpRequest(httpRequest);
                HttpResponse res = new HttpResponse(httpResponse);
                
                // try to route the request
                try {
                    var match = Validation.PathRegex.Match(httpRequest.Url.ToString());
                    
                    string requestPath = "Could not parse";
                    
                    HttpMethod? method = httpRequest.HttpMethod.ToHttpMethod();
                    
                    // if path found and method parsed
                    if (match.Success && method != null) {
                        // re-add preceding slash to path
                        requestPath = String.Format("/{0}", match.ToString());
                        
                        // if we have a trailing slash, for whatever reason (sender error)
                        if (requestPath.Length > 1 && requestPath[requestPath.Length - 1] == '/') {
                            requestPath = requestPath.Substring(0, requestPath.Length - 1);
                        }
                        
                        // try to route to a responder
                        bool didFindRoute = router.RunRoute(
                            method.Value,
                            requestPath,
                            req,
                            res
                        );
                        
                        // if no matching responders were found
                        if (!didFindRoute) {
                            // respond with the server's 404 responder
                            Server.Responder404(req, res, new List<string>());
                        }
                    } else {
                        // if path could not be parsed or method not recognized
                        res.StatusCode = (int) HttpStatusCode.InternalServerError;
                    }
                    
                    // close the output stream, as a good habit
                    httpResponse.OutputStream.Close();
                    
                    // stop the stopwatch to get a reading
                    stopwatch.Stop();
                    
                    // log request diagnostics
                    Log.Information(
                        "[Asypi] {0} {1}: {2} => {3} {4} ({5})",
                        httpRequest.RemoteEndPoint,
                        httpRequest.HttpMethod,
                        httpRequest.Url,
                        requestPath,
                        res.StatusCode,
                        String.Format("{0}ms", stopwatch.ElapsedMilliseconds)
                    );
                } catch (Exception e) {
                    // if an unhandled exception happened, fail semi-silently
                    // so that this worker can continue responding to requests
                    Log.Error("Unhandled exception occurred during handling of request: {0} {1}", e.Message, e.StackTrace);
                    
                    // close connection to client so that they aren't hanging
                    res.Abort();
                }
            
            });
        }
    }
}
