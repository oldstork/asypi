using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Serilog;

namespace Asypi {
    
    public enum LogLevel {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }
    
    public class Server {
        public int Port { get; private set; }
        public IEnumerable Hosts { get; private set; }
        public int WorkerCount { get; private set; }
        public LogLevel LogLevel { get; private set; }
        Responder responder404 = ResponderUtils.Respond404;
        
        Router router;
        
        
        public Server() {
            Init(8000, new string[]{ "localhost" }, 512, LogLevel.Debug);
        }
        
        public Server(int port, string hostname) {
            Init(port, new string[]{ hostname }, 512, LogLevel.Debug);
        }
        
        
        void Init(int port, IEnumerable hosts, int workerCount, LogLevel logLevel) {
            Port = port;
            Hosts = hosts;
            WorkerCount = workerCount;
            LogLevel = logLevel;
            
            LoggerConfiguration logConfig = new LoggerConfiguration();
            
            switch (LogLevel) {
                case LogLevel.Debug:
                    logConfig.MinimumLevel.Debug();
                    break;
                case LogLevel.Information:
                    logConfig.MinimumLevel.Information();
                    break;
                case LogLevel.Warning:
                    logConfig.MinimumLevel.Warning();
                    break;
                case LogLevel.Error:
                    logConfig.MinimumLevel.Error();
                    break;
                case LogLevel.Fatal:
                    logConfig.MinimumLevel.Fatal();
                    break;
            }
            
            Serilog.Log.Logger = logConfig.WriteTo.Console().CreateLogger();
            
            router = new Router();
        }
        
        public void AddRoute(HttpMethod method, string path, Responder responder) {
            router.AddRoute(method, path, responder);
        }
        
        public void AddRoute(HttpMethod method, string path, SimpleTextResponder responder, string contentType) {
            router.AddRoute(method, path, responder, contentType);
        }
        
        public void AddRoute(HttpMethod method, string path, SimpleTextResponderArgs responder, string contentType) {
            router.AddRoute(method, path, responder, contentType);
        }
        
        public void AddRoute(HttpMethod method, string path, ComplexTextResponder responder, string contentType) {
            router.AddRoute(method, path, responder, contentType);
        }
        
        public void Set404Responder(Responder responder) {
            responder404 = responder;
        }
        
        public void Set404Responder(SimpleTextResponder responder, string contentType) {
            responder404 = ResponderUtils.Transform(responder, contentType);
        }
        
        public void Set404Responder(ComplexTextResponder responder, string contentType) {
            responder404 = ResponderUtils.Transform(responder, contentType);
        }
        
        public Task RunAsync() {
            return Task.Run(() => {
                HttpListener listener = new HttpListener();
                
                foreach (string host in Hosts) {
                    if (Validation.IsHostnameValid(host)) {
                        string prefix = String.Format("http://{0}:{1}/", host, Port);
                        
                        Log.Information("[Asypi] Binding to {0}", prefix);
                        
                        listener.Prefixes.Add(prefix);
                    } else {
                        Log.Fatal("[Asypi] Invalid hostname: {0}", host);
                        throw new Exception(String.Format("Invalid hostname: {0}", host));
                    }
                }
                
                Log.Information("[Asypi] Starting server with {0} workers", WorkerCount);
                
                listener.Start();
                
                for (int i = 0; i < WorkerCount; i++) {
                    Task.Run(async () => {
                        while (true) {
                            HttpListenerContext context = await listener.GetContextAsync();
                            
                            HttpListenerRequest httpRequest = context.Request;
                            HttpListenerResponse httpResponse = context.Response;
                            
                            HttpRequest req = new HttpRequest(httpRequest);
                            HttpResponse res = new HttpResponse(httpResponse);
                            
                            var match = Validation.PathRegex.Match(httpRequest.Url.ToString());
                            
                            string requestPath = "Could not parse";
                            
                            if (match.Success) {
                                requestPath = String.Format("/{0}", match.ToString());
                                
                                if (requestPath.Length > 1 && requestPath[requestPath.Length - 1] == '/') {
                                    requestPath = requestPath.Substring(0, requestPath.Length - 1);
                                }
                                
                                bool foundRoute = router.Route(
                                    httpRequest.HttpMethod.ToHttpMethod().Value,
                                    requestPath,
                                    req,
                                    res
                                );
                                
                                if (!foundRoute) {
                                    responder404(req, res, new List<string>());
                                }
                                
                            }
                            
                            httpResponse.OutputStream.Close();
                            
                            Log.Debug(
                                "[Asypi] {0} {1}: {2} => {3} {4}",
                                httpRequest.RemoteEndPoint,
                                httpRequest.HttpMethod,
                                httpRequest.Url,
                                requestPath,
                                res.StatusCode
                            );
                        }
                    });
                }
                
                while (true) {
                    // let workers work
                }
            });
        }
        
        /// <summary>Runs the server. This will block the main thread until the server stops running. For async, consider <c>Server.RunAsync().</c></summary>
        public void Run() {
            RunAsync().Wait();
        }
    }
}
