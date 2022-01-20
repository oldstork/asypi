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

// Serve files from ./static at /staticfiles
server.RouteStaticDir("/staticfiles", "./static");

server.Run();
```

## Cache-Control

When using `Asypi.RouteStaticFile()` or `Asypi.RouteStaticDir()`, the `Cache-Control` header is automatically set to the following:

```text
Cache-Control: public, max-age=86400
```

## File Compression

With a directory structure such as the following:

```text
static
+---foo.txt
+---foo.txt.br
+---foo.txt.gz
```

`Asypi.RouteStaticFile()` and `Asypi.RouteStaticDir()` will first try to satisfy requests to `foo.txt` with `foo.txt.br`, followed by `foo.txt.gz`. Asypi automatically checks the `Accept-Encoding` header and automatically sets the `Content-Encoding` header to ensure compatibility.

## Using `FileServer`

FileServer allows quick, easy, and cached access to files on disk. `Server.RouteStaticFile()` and `Server.RouteStaticDir()` both utilize `FileServer` under the hood.

`FileServer` caches files under an LFU system. The maximum size of the cache and the interval between cache updates can be configured when creating a `Server`. Note that `FileServer` will *not* cache files larger than 50% of the maximum cache size.

```C#
using Asypi;

Server server = new Server(8000, "localhost");

server.Route(
    HttpMethod.Get,
    "/",
    (Req req, Res res) => {
        byte[] content = FileServer.Get("content.txt");
        string decodedContent = Encoding.UTF8.GetString(content);
        // do something
    }
);

server.Run();
```
