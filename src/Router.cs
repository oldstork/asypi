using System;
using System.Collections.Generic;
using System.Net;

using Serilog;

namespace Asypi {
    
    /// <summary>Represents a route and its associated <see cref="Responder"/>.</summary>
    class Route {
        /// <summary>The individual parts of the path, delimited in requests by <c>/</c>.</summary>
        List<string> subpaths = new List<string>();
        
        /// <summary>The associated <see cref="Responder"/> of this <see cref="Route"/>.</summary>
        Responder responder;
        
        /// <summary>The <see cref="HttpMethod"/> to which this route will respond.</summary>
        public HttpMethod Method { get; private set; }
        
        /// <summary>The path(s) to which this route will respond.</summary>
        public string Path { get; private set; }
        
        /// <summary>The number of subpaths of this <see cref="Route"/>'s path.</summary>
        public int Length { get { return subpaths.Count; } }
        
        /// <summary>Whether or not this <see cref="Route"/>'s path is parameterized.</summary>
        public bool Parameterized { get; private set; }
        
        /// <summary>
        /// Creates a new <see cref="Route"/> from a given
        /// <see cref="HttpMethod"/>, path, and <see cref="Responder"/>.
        /// </summary>
        public Route(HttpMethod method, string path, Responder responder) {
            this.responder = responder;
            
            this.Method = method;
            this.Path = path;
            
            Parameterized = false;
            
            // split path into its parts
            List<string> split = Utils.SplitPath(path);
            
            // for each part of the path
            foreach (string substr in split) {
                // if its a real thing and not an empty string/whitespace/whatever
                if (substr.Length > 0) {
                    if (substr[0] == '{' && substr[substr.Length - 1] == '}') {
                        // if its a variable parameter
                        Parameterized = true;
                        subpaths.Add(null);
                    } else {
                        // if its hard-coded
                        subpaths.Add(substr);
                    }
                }
            }
            
        }
        
        /// <summary>Attempts to match a given path to this route.
        /// Returns args if successfully matched, otherwise returns null.</summary>
        public List<string> Match(List<string> splitPath) {
            if (Parameterized) {
                // only match if the routes length and the length of the path we are matching against are the same
                if (subpaths.Count != splitPath.Count) {
                    Log.Error("[Asypi] Route.Match() called with a path of non-matching length");
                    return null;
                }
                
                // list of values provided for variable arguments
                List<string> args = new List<string>();
                
                for (int i = 0; i < subpaths.Count; i++) {
                    if (subpaths[i] == null) {
                        // if this is a variable parameter, accept whatever
                        args.Add(splitPath[i]);
                    } else if (subpaths[i] != splitPath[i]) {
                        // if these arent equal and this isnt a variable parameter
                        return null;
                    }
                }
                
                return args;
            } else {
                Log.Warning("[Asypi] Route.Match() called on a non-parameterized route");
                
                return null;
            }
        }
        
        /// <summary>Runs the associated <see cref="Responder"/> of the <see cref="Route"/> with the provided arguments.</summary>
        public void Run(HttpRequest req, HttpResponse res, List<string> args) {
            responder(req, res, args);
        }
    }
    
    /// <summary>Routes paths to <see cref="Responder"/>s.</summary>
    class Router {
        /// <summary>Routes by method and then size</summary>
        Dictionary<HttpMethod, Dictionary<int, List<Route>>> routes =
            new Dictionary<HttpMethod, Dictionary<int, List<Route>>>();
        
        /// <summary>Non-parameterized routes by path</summary>
        Dictionary<HttpMethod, Dictionary<string, Route>> nonParameterizedRoutes = 
            new Dictionary<HttpMethod, Dictionary<string, Route>>();
        
        /// <summary>Creates a new <see cref="Router"/>.</summary>
        public Router() {
            Log.Information("[Asypi] Initializing router");
            
            foreach (HttpMethod method in Utils.PossibleValues<HttpMethod>()) {
                routes[method] = new Dictionary<int, List<Route>>();
                nonParameterizedRoutes[method] = new Dictionary<string, Route>();
                
                for (int i = 0; i < Params.MAX_PATH_LENGTH; i++) {
                    routes[method][i] = new List<Route>();
                }
            }
        }
        
        /// <summary>Create a new route with the given parameters.</summary>
        public void Route(HttpMethod method, string path, Responder responder) {
            Route route = new Route(method, path, responder);
            routes[method][route.Length].Add(route);
            
            if (!route.Parameterized) {
                nonParameterizedRoutes[method][route.Path] = route;
            }
            
            Log.Debug("[Asypi] Registered route {0} {1}", method.AsString(), path);
        }
        
        /// <summary>Create a new route with the given parameters.</summary>
        public void Route(HttpMethod method, string path, SimpleTextResponder responder, string contentType) {
            Route(method, path, ResponderUtils.Transform(responder, contentType));
        }
        
        /// <summary>Create a new route with the given parameters.</summary>
        public void Route(HttpMethod method, string path, SimpleTextResponderArgs responder, string contentType) {
            Route(method, path, ResponderUtils.Transform(responder, contentType));
        }
        
        /// <summary>Create a new route with the given parameters.</summary>
        public void Route(HttpMethod method, string path, ComplexTextResponder responder, string contentType) {
            Route(method, path, ResponderUtils.Transform(responder, contentType));
        }
        
        /// <summary>
        /// Runs the route matching <c>method</c> and <c>path</c> with the given parameters.
        /// <br />
        /// Returns true if a path was found and run, and false otherwise.
        /// </summary>
        public bool RunRoute(HttpMethod method, string path, HttpRequest req, HttpResponse res) {
            // check if path corresponds to a non-parameterized route first
            if (nonParameterizedRoutes[method].ContainsKey(path)) {
                // if we found a corresponding route, go and run it
                Route route = nonParameterizedRoutes[method][path];
                
                route.Run(req, res, new List<string>());
                
                return true;
            } else {
                // if we can't find a non-parameterized route, first get the length of the path
                List<string> split = Utils.SplitPath(path);
                
                // then match against parameterized routes of same method and same route length
                foreach (Route route in routes[method][split.Count]) {
                    if (route.Parameterized) {
                        List<string> args = route.Match(split);
                        
                        // if route matched
                        if (args != null) {
                            // run the route
                            route.Run(req, res, args);
                            
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
    }
}
