# Serving Static Files

Asypi comes with both powerful utilities for routing static files and a versatile `FileServer` for finer control.

## Serving Single Files

```C#
using Asypi;

Server server = new Server(8000, "localhost");

// Serve content.txt at /
server.RouteStaticFile("/", "content.txt");

server.Run();
```

## Serving Directories

```C#
using Asypi;

Server server = new Server(8000, "localhost");

// Serve files under ./static at /staticfiles
server.RouteStaticDir("/staticfiles", "./static");

server.Run();
```

## Using `FileServer`

FileServer allows quick, easy, and cached access to files on disk. `Server.RouteStaticFile()` and `Server.RouteStaticDir()` both utilize `FileServer` under the hood.

`FileServer` caches files under an LRU system.

```C#
using Asypi;

Server server = new Server(8000, "localhost");

server.Route(
    HttpMethod.Get,
    "/",
    (HttpRequest req, HttpResponse res, List<string> args) => {
        byte[] content = FileServer.Get("content.txt");
        string decodedContent = Encoding.UTF8.GetString(content);
        // do something
    }
);

server.Run();
```