# Setup, Routing, and Responders

## Setup

Asypi makes setting up a server almost effortless. For example, to set up and run a server listening on `localhost:8000`, one can use the following:

```C#
using Asypi;

Server server = new Server(8000, "localhost");

server.Run();
```

More constructors for `Server` are available in the API reference.

`Server.Run()` will block the main thread until the server terminates. If you need to do other things while the server runs, consider `Server.RunAsync()`. For example:

```C#
using Asypi;

Server server = new Server(8000, "localhost");

server.RunAsync();
DoOtherThings();
```

Beware that when the main thread terminates, the server does too.

## Routing

Asypi makes setting up routes a simple and efficient process.

### Simple Routes

```C#
using Asypi;

Server server = new Server(8000, "localhost");

// Respond to GET requests to / with body "Hello World!" and content type flag "text/html"
server.Route(
    HttpMethod.Get,
    "/",
    () => { return "Hello World!"; },
    "text/html"
);

server.Run();
```

### Parameterized Routes

Variable parameter names must be surrounded by curly braces.

```C#
using Asypi;

Server server = new Server(8000, "localhost");

// Respond to GET requests to /{name} with a personalized greeting
server.Route(
    HttpMethod.Get,
    "/{name}",
    (List<string> args) => { 
        return String.Format(
            "Hello {0}!",
            args[0]
        );
    },
    "text/html"
);

/*

Expected Results:
/Joe -> "Hello Joe!"
/Zoe -> "Hello Zoe!"
/123 -> "Hello 123!"

*/

server.Run();
```

## Responders

In Asypi, each route is assigned a responder that fires whenever the route is accessed. A basic responder has the following signature and returns `void`:

```C#
(HttpRequest res, HttpResponse res, List<string> args) => {
    // do something
}
```

For convenience, a number of other responder signatures are supported out of the box:

```C#
() => {
    // do something
    return body;
}

(List<string> args) => {
    // do something
    return body;
}

(HttpRequest req, HttpResponse res, List<string> args) => {
    // do something
    return body;
}
```

When attaching any of these to a route, a content type must also be provided since the responder cannot reasonably infer one.
