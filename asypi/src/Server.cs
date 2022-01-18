using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Serilog;

namespace Asypi {
    
    /// <summary>
    /// Represents the minimum log level at which
    /// the logger will forward a log to its sinks.
    /// Corresponds directly to Serilog log levels.
    /// </summary>
    public enum LogLevel {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }
    
    
    /// <summary>
    /// Prevents creation of multiple servers,
    /// which would be bad because servers modify 
    /// global variables within the Asypi namespace
    /// for user convenience purposes.
    /// </summary>
    static class ServerGuard {
        /// <summary>Whether or not a server has already been created.</summary>
        public static bool ServerExists = false;
    }
    
    /// <summary>Provides a simple, configurable, and scalable HTTP server.</summary>
    public class Server {
        /// <summary>
        /// The port that the <see cref="Server"/> will bind to when
        /// <see cref="Server.Run()"/> or <see cref="Server.RunAsync()"/> is called.
        /// </summary>
        public int Port { get; private set; }
        
        /// <summary>
        /// The hosts that the <see cref="Server"/> will bind to when
        /// <see cref="Server.Run()"/> or <see cref="Server.RunAsync()"/> is called.
        /// </summary>
        public IEnumerable Hosts { get; private set; }
        
        /// <summary>
        /// The <c>LogLevel</c> that the <see cref="Server"/>
        /// initialized the logger with.
        /// </summary>
        public LogLevel LogLevel { get; private set; }
        
        /// <summary>
        /// Whether the <see cref="Server"/>
        /// should compress responses if possible.
        /// Can be changed after Server is initialized.
        /// </summary>
        public bool CompressResponses { get; set; }
        
        /// <summary>
        /// The maximum LFU cache size, in MiB, that the <see cref="Server"/>
        /// initialized <c>FileServer</c> with.
        /// </summary>
        public int LFUCacheSize { get; private set; }
        
        /// <summary>
        /// The epoch length, in milliseconds, that the <see cref="Server"/>
        /// initialized <c>FileServer</c> with.
        /// </summary>
        public int FileServerEpochLength { get; private set; }
        
        
        /// <summary>
        /// The <c>Responder</c> that the <see cref="Server"/>
        /// will route requests to when no other valid routes were found.
        /// </summary>
        public Responder Responder404 { get; private set; }
        
        /// <summary>The internal router of the <see cref="Server"/>.</summary>
        Router router;
        
        
        /// <summary>Returns the current version of Asypi as a string.</summary>
        public static string AsypiVersion() {
            return Params.ASYPI_VERSION;
        }
        
        
        /// <summary>
        /// Creates a new <see cref="Server"/> with the provided parameters.
        /// <c>LFUCacheSize</c> is measured in MiB, and <c>fileServerEpochLength</c> is measured in milliseconds.
        /// <br />
        /// If provided <c>null</c>, the following parameters will be set to default values:
        /// <c>LFUCacheSize</c>, <c>fileServerEpochLength</c>
        /// </summary>
        protected void ConstructorDoc() {}
        
        
        /// <inheritdoc cref="ConstructorDoc" />
        public Server(
            int port = 8000,
            string hostname = "localhost",
            LogLevel logLevel = LogLevel.Debug,
            bool compressResponses = true,
            int? LFUCacheSize = null,
            int? fileServerEpochLength = null
        ) {
            Init(port, new string[]{ hostname }, logLevel, compressResponses, LFUCacheSize, fileServerEpochLength);
        }
        
        /// <inheritdoc cref="ConstructorDoc" />
        public Server(
            int port,
            string[] hostnames,
            LogLevel logLevel = LogLevel.Debug,
            bool compressResponses = true,
            int? LFUCacheSize = null,
            int? fileServerEpochLength = null
        ) {
            Init(port, hostnames, logLevel, compressResponses, LFUCacheSize, fileServerEpochLength);
        }
        
        
        /// <summary>
        /// Internal initializer of the <see cref="Server"/>.
        /// Allows multiple user-facing constructors without reusing code.
        /// </summary>
        void Init(int port, IEnumerable hosts, LogLevel logLevel, bool compressResponses, int? LFUCacheSize, int? fileServerEpochLength) {
            // Check if another server has already been created
            if (ServerGuard.ServerExists) {
                Log.Fatal(
                    @"[Asypi] Only one server may run at a time.
                    For more information, view the Server class under the API reference."
                );
                
                throw new Exception("Attempted to create multiple instances of Asypi.Server");
            }
            
            ServerGuard.ServerExists = true;
            
            
            // Set server configuration variables
            this.Port = port;
            this.Hosts = hosts;
            this.LogLevel = logLevel;
            this.CompressResponses = compressResponses;
            
            if (LFUCacheSize != null) {
                this.LFUCacheSize = LFUCacheSize.Value;
            } else {
                this.LFUCacheSize = Params.DEFAULT_FILESERVER_LFU_CACHE_SIZE;
            }
            
            if (fileServerEpochLength != null) {
                this.FileServerEpochLength = fileServerEpochLength.Value;
            } else {
                this.FileServerEpochLength = Params.DEFAULT_FILESERVER_EPOCH_LENGTH;
            }
            
            
            // Initialize logger
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
            
            
            // Bounds checking
            if (this.LFUCacheSize < 1) {
                Log.Fatal("[Asypi] Attempted to set LFU cache size to a number less than 1");
                throw new Exception("Received invalid LFU cache size");
            } else if (this.LFUCacheSize > 1024) {
                Log.Warning(
                    "[Asypi] Setting LFU cache size to {0} MiB; ensure system has sufficient memory",
                    this.LFUCacheSize
                );
            }
            
            
            // Initialize LFU cache
            FileServer.Init(this.FileServerEpochLength, this.LFUCacheSize);
            
            // Initialize router
            // This has to come after logger initialization
            // because router initialization outputs some logs
            Responder404 = ResponderUtils.Respond404;
            router = new Router();
        }
        
        /// <summary>Resets the server, including the associated <see cref="Router" /> and the <see cref="FileServer" />.</summary>
        public void Reset() {
            Log.Warning("[Asypi] Resetting server");
            
            router.Reset();
            FileServer.Reset();
        }
        
        /// <summary>
        /// <br />
        /// Paths should not contain trailing slashes.
        /// <br />
        /// Paths can include variable parameters, the values of which will be forwarded to the responder.
        /// Variable parameters must be surrounded by curly braces.
        /// For example, <c>/users/{id}</c> would match <c>/users/1</c>, <c>/users/joe</c>, etc.,
        /// and the responder would receive argument lists <c>["1"]</c> and <c>["joe"]</c> respectively.
        /// </summary>
        protected void PathsDoc() {}
        
        /// <summary>
        /// Routes requests to <c>path</c> of method <c>method</c> to <c>responder</c>.
        /// <inheritdoc cref="Server.PathsDoc" />
        /// </summary>
        public void Route(HttpMethod method, string path, Responder responder) {
            router.Route(method, path, responder);
        }
        
        /// <summary>
        /// Routes requests to <c>path</c> of method <c>method</c> to a simple responder that sets
        /// the body of the response to the output of the provided function, and the content type
        /// to <c>contentType</c>.
        /// <br />
        /// If no headers are provided, will use default headers.
        /// <inheritdoc cref="Server.PathsDoc" />
        /// </summary>
        protected void TransformedResponderRouteDoc() {}
        
        /// <inheritdoc cref="Server.TransformedResponderRouteDoc" />
        public void Route(
            HttpMethod method,
            string path,
            SimpleTextResponder responder,
            string contentType,
            IHeaders headers = null
        ) {
            router.Route(method, path, responder, contentType, headers);
        }
        
        /// <inheritdoc cref="Server.TransformedResponderRouteDoc" />
        public void Route(
            HttpMethod method,
            string path,
            ComplexTextResponder responder,
            string contentType,
            IHeaders headers = null
        ) {
            router.Route(method, path, responder, contentType, headers);
        }
        
        
        /// <summary>Note that <see cref="DefaultHeaders"/> contains <c>X-Content-Type-Options: nosniff</c>.</summary>
        protected void DefaultHeadersNoteDoc() {}
        
        /// <summary>
        /// Routes requests to <c>path</c> to the file at <c>filePath</c>.
        /// The response will have its content type set to <c>contentType</c> if provided.
        /// Otherwise, the content type will be guessed.
        /// <br />
        /// <inheritdoc cref="Server.DefaultHeadersNoteDoc" />
        /// </summary>
        public void RouteStaticFile(string path, string filePath, string contentType = null) {
            router.Route(HttpMethod.Get, path, (Req req, Res res) => {
                byte[] bytes;
                
                IHeaders headers;
                
                
                if (CompressResponses) {
                    // get Accept-Encoding value
                    string[] acceptedEncodings = req.Headers.GetValues("Accept-Encoding");
                    
                    if (acceptedEncodings != null) {
                        // check if br or gz accepted
                        bool brAccepted = false;
                        bool gzAccepted = false;
                        
                        foreach (string enc in acceptedEncodings) {
                            if (enc == "br") {
                                brAccepted = true;
                            } else if (enc == "gzip") {
                                gzAccepted = true;
                            }
                        }
                        
                        if (brAccepted && File.Exists(filePath + ".br")) {
                            // respond with br, if possible
                            bytes = FileServer.Get(filePath + ".br");
                            
                            DefaultHeadersMut newHeaders = new DefaultHeadersMut();
                            newHeaders.Set("Content-Encoding", "br");
                            headers = newHeaders;
                        } else if (gzAccepted && File.Exists(filePath + ".gz")) {
                            // then try gz
                            bytes = FileServer.Get(filePath + ".gz");
                            
                            DefaultHeadersMut newHeaders = new DefaultHeadersMut();
                            newHeaders.Set("Content-Encoding", "gzip");
                            headers = newHeaders;
                        } else {
                            // if none, then just respond with file
                            headers = DefaultServerHeaders.Instance;
                            
                            bytes = FileServer.Get(filePath);
                        }
                    } else {
                        // if no accept-encoding header
                        headers = DefaultServerHeaders.Instance;
                        
                        bytes = FileServer.Get(filePath);
                    }
                } else {
                    // if not compressing
                    headers = DefaultServerHeaders.Instance;
                    
                    bytes = FileServer.Get(filePath);
                }
                
                // now that we have bytes and headers, send a response
                if (bytes == null) {
                    Responder404(req, res);
                } else {
                    res.ContentType = MimeGuesser.GuessTypeByExtension(filePath);
                    
                    res.LoadHeaders(headers);
                    
                    res.BodyBytes = bytes;
                }
            });
        }
        
        /// <summary>
        /// Routes requests to paths matching <c>match</c> under <c>mountRoot</c> to files under <c>dirRoot</c>.
        /// Content types will be guessed.
        /// <br />
        /// If <c>maxDepth</c> is not set, will recursively include all subdirectories of <c>dirRoot</c>.
        /// Otherwise, will only include subdirectories to a depth of <c>maxDepth</c>.
        /// For example, with <c>maxDepth</c> set to <c>1</c>, will only include files directly under <c>dirRoot</c>.
        /// <br />
        /// <inheritdoc cref="Server.DefaultHeadersNoteDoc" />
        /// <br />
        /// If finer control is necessary, consider mounting individual files using <c>Server.RouteStaticFile()</c>.
        /// </summary>
        public void RouteStaticDir(string mountRoot, string dirRoot, int? maxDepth = null, string match = ".*") {
            // It's possible to have a trailing slash if the provided mountRoot is /
            string eMountRoot = mountRoot[mountRoot.Length - 1] == '/' ? mountRoot.Substring(0, mountRoot.Length - 1) : mountRoot;
            
            // grab file paths in dirRoot
            string[] files = new string[]{};
            
            try {
                files = Directory.GetFiles(dirRoot);
            } catch (DirectoryNotFoundException) {
                Log.Error("[Asypi] Server.RouteStaticDir() Directory not found: {0}", dirRoot);
                return;
            }
            
            Regex regex = new Regex(match);
            
            // for every file in dirRoot
            foreach (string filePath in files) {
                // match against pattern
                if (regex.Match(filePath).Success) {
                    // get the filename only and mount it
                    // this works because when we recursively call RouteStaticDir(),
                    // we also change mountRoot
                    string[] filePathSplit = Utils.SplitFilePath(filePath);
                    
                    string mountPath = String.Format("{0}/{1}", eMountRoot, filePathSplit[filePathSplit.Length - 1]);
                    
                    RouteStaticFile(mountPath, filePath);
                }
            }
            
            
            // If we should also search directories under dirRoot
            if (maxDepth == null || maxDepth > 1) {
                // grab directory paths in dirRoot
                string[] dirs = new string[]{};
                
                try {
                    dirs = Directory.GetDirectories(dirRoot);
                } catch (DirectoryNotFoundException) {
                    // This theoretically should not happen, because the earlier one should fire first and return
                    // However it's still here for safety
                    Log.Error("[Asypi] Server.RouteStaticDir() Directory not found: {0}", dirRoot);
                    return;
                }
                
                // for every directory in dirRoot
                foreach (string dirPath in dirs) {
                    // get only the directory name
                    string[] dirPathSplit = Utils.SplitFilePath(dirPath);
                    
                    // mount files under the directory
                    RouteStaticDir(
                        String.Format("{0}/{1}", eMountRoot, dirPathSplit[dirPathSplit.Length - 1]),
                        dirPath,
                        maxDepth == null ? null : maxDepth - 1
                    );
                }
            }
        }
        
        /// <summary>Sets <see cref="Server.Responder404"/> to the provided <see cref="Responder"/>.</summary>
        public void Set404Responder(Responder responder) {
            Responder404 = responder;
        }
        
        /// <summary>
        /// Sets <see cref="Server.Responder404"/> to a simple responder that sets
        /// the body of the response to the output of the provided function, and the content type
        /// to <c>contentType</c>.
        /// </summary>
        public void Set404Responder(SimpleTextResponder responder, string contentType) {
            Responder404 = ResponderUtils.Transform(responder, contentType, null);
        }
        
        /// <summary>
        /// Sets <see cref="Server.Responder404"/> to a simple responder that sets
        /// the body of the response to the output of the provided function, and the content type
        /// to <c>contentType</c>.
        /// </summary>
        public void Set404Responder(ComplexTextResponder responder, string contentType) {
            Responder404 = ResponderUtils.Transform(responder, contentType, null);
        }
        
        
        /// <summary>
        /// Registers the middleware for use on all paths matching
        /// <c>matchExpression</c>.
        /// </summary>
        public void Use(string matchExpression, Middleware middleware) {
            router.Use(matchExpression, middleware);
        }
        
        /// <summary>Runs the server. For a sync wrapper, consider <see cref="Server.Run()"/>.</summary>
        public Task RunAsync() {
            return Task.Run(() => {
                // initialize HttpListener
                HttpListener listener = new HttpListener();
                
                // generate HttpListener prefixes from provided hostnames and port
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
                
                Log.Information("[Asypi] Starting server");
                
                listener.Start();
                
                while (true) {
                    // whenever we get a request, make a worker to handle it
                    HttpListenerContext context = listener.GetContext();
                    Worker worker = new Worker(this, router);
                    worker.Run(context);
                }
            });
        }
        
        /// <summary>Runs the server. This will block the thread until the server stops running. For async, consider <see cref="Server.RunAsync()"/>.</summary>
        public void Run() {
            RunAsync().Wait();
        }
    }
}
