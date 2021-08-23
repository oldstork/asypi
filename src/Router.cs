using System;
using System.Collections.Generic;
using System.Net;

using Serilog;

namespace Asypi {
    
    class Route {
        List<string> subpaths = new List<string>();
        Responder responder;
        
        public HttpMethod Method { get; private set; }
        public string Path { get; private set; }
        public int Length { get { return subpaths.Count; } }
        public bool Parameterized { get; private set; }
        
        public Route(HttpMethod method, string path, Responder responder) {
            this.responder = responder;
            
            this.Method = method;
            this.Path = path;
            
            Parameterized = false;
            
            List<string> split = Utils.SplitPath(path);
            
            foreach (string substr in split) {
                if (substr.Length > 0) {
                    if (substr[0] == '{' && substr[substr.Length - 1] == '}') {
                        Parameterized = true;
                        subpaths.Add(null);
                    } else {
                        subpaths.Add(substr);
                    }
                }
            }
            
        }
        
        /// <summary>Attempts to match a given path to this route.
        /// Returns args if successfully matched, otherwise returns null.</summary>
        public List<string> Match(List<string> splitPath) {
            if (Parameterized) {
                if (subpaths.Count != splitPath.Count) {
                    Log.Error("[Asypi] Route.Match() called with a path of non-matching length");
                    return null;
                }
                
                List<string> args = new List<string>();
                
                for (int i = 0; i < subpaths.Count; i++) {
                    if (subpaths[i] == null) {
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
        
        public void Run(HttpRequest req, HttpResponse res, List<string> args) {
            responder(req, res, args);
        }
    }
    
    public class Router {
        /// <summary>Routes by method and then size</summary>
        Dictionary<HttpMethod, Dictionary<int, List<Route>>> routes =
            new Dictionary<HttpMethod, Dictionary<int, List<Route>>>();
        
        Dictionary<HttpMethod, Dictionary<string, Route>> nonParameterizedRoutes = 
            new Dictionary<HttpMethod, Dictionary<string, Route>>();
        
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
        
        public void AddRoute(HttpMethod method, string path, Responder responder) {
            Route route = new Route(method, path, responder);
            routes[method][route.Length].Add(route);
            
            if (!route.Parameterized) {
                nonParameterizedRoutes[method][route.Path] = route;
            }
        }
        
        public void AddRoute(HttpMethod method, string path, SimpleTextResponder responder, string contentType) {
            AddRoute(method, path, ResponderUtils.Transform(responder, contentType));
        }
        
        public void AddRoute(HttpMethod method, string path, SimpleTextResponderArgs responder, string contentType) {
            AddRoute(method, path, ResponderUtils.Transform(responder, contentType));
        }
        
        public void AddRoute(HttpMethod method, string path, ComplexTextResponder responder, string contentType) {
            AddRoute(method, path, ResponderUtils.Transform(responder, contentType));
        }
        
        /// Returns true if successful, and false otherwise
        public bool Route(HttpMethod method, string path, HttpRequest req, HttpResponse res) {
            if (nonParameterizedRoutes[method].ContainsKey(path)) {
                Route route = nonParameterizedRoutes[method][path];
                
                route.Run(req, res, new List<string>());
                
                return true;
            } else {
                List<string> split = Utils.SplitPath(path);
                
                foreach (Route route in routes[method][split.Count]) {
                    if (route.Parameterized) {
                        List<string> args = route.Match(split);
                        
                        if (args != null) {
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
