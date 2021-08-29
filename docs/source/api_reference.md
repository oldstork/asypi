# API Reference

## **Server**

The `Server` class is the heart of Asypi. `Server`s handle receiving requests and sending responses, routing requests, managing routes and responders, and logging diagnostic information.

Due to the way that `Server` interfaces with the static `FileServer`, only one `Server` may be created per run.

### Public Fields

#### `int Port`

The port that the `Server` will bind to when `Server.Run()` or `Server.RunAsync()` is called.

---

#### `IEnumerable Hosts`

The hosts that the `Server` will bind to when `Server.Run()` or `Server.RunAsync()` is called.

---

#### `LogLevel LogLevel`

The `LogLevel` that the `Server` initialized the logger with.

---

#### `int LRUCacheSize`

The LRU cache size that the `Server` initialized `FileServer` with.

---

#### `Responder Responder404`

The `Responder` that the `Server` will route requests to when no other valid routes were found.

### Constructors

`public Server(int port = 8000, string hostname = "localhost", LogLevel logLevel = LogLevel.Debug, int LRUCacheSize = 64)`

`public Server(int port, string[] hostnames, LogLevel logLevel = LogLevel.Debug, int LRUCacheSize = 64)`

### Routing Methods

Routes requests to `path` of method `method` to `responder`.

Paths should not contain trailing slashes.

Paths can include variable parameters, the values of which will be forwarded to the responder. Variable parameters must be surrounded by curly braces. For example, `/users/{id}` would match `/users/1`, `/users/joe`, etc., and the responder would receive argument lists `["1"]` and `["joe"]` respectively.

`void Route(HttpMethod method, string path, Responder responder)`

`void Route(HttpMethod method, string path, SimpleTextResponder responder, string contentType, IHeaders headers = null)`

`void Route(HttpMethod method, string path, SimpleTextResponderArgs responder, string contentType, IHeaders headers = null)`

`void Route(HttpMethod method, string path, ComplexTextResponder responder, string contentType, IHeaders headers = null)`

### Static File Routing

#### `void RouteStaticFile(string path, string filePath, string contentType = null)`

Routes requests to path to the file at `filePath`. The response will have its content type set to `contentType` if provided. Otherwise, the content type will be guessed.

Note that `DefaultHeaders` contains `X-Content-Type-Options: nosniff`.

---

#### `RouteStaticDir(string mountRoot, string dirRoot, int? maxDepth = null, string match = ".*")`

Routes requests to paths matching `match` under `mountRoot` to files under `dirRoot`. Content types will be guessed.

If `maxDepth` is not set, will recursively include all subdirectories of `dirRoot`. Otherwise, will only include subdirectories to a depth of `maxDepth`. For example, with `maxDepth` set to `1`, will only include files directly under `dirRoot`.

Note that `DefaultHeaders` contains `X-Content-Type-Options: nosniff`.

If finer control is necessary, consider mounting individual files using `Server.RouteStaticFile()`.

### 404 Responder Setters

`void Set404Responder(Responder responder)`

`void Set404Responder(SimpleTextResponder responder, string contentType)`

`void Set404Responder(ComplexTextResponder responder, string contentType)`

### Other Methods

#### `void Reset()`

Resets the server. This will remove all previously registered routes.

Exists primarily for testing purposes.

---

#### `Task RunAsync()`

Runs the server. For a sync wrapper, consider `Server.Run()`.

---

#### `void Run()`

Runs the server. This will block the thread until the server stops running. For async, consider `Server.RunAsync()`.

## **Responders**

Under the hood, all requests in Asypi are handled by a responder with the following delegate:

```C#
public delegate void Responder(
    HttpRequest request,
    HttpResponse response,
    List<string> args
);
```

However, for convenience, simplified responders are also generally accepted. These are:

```C#
public delegate string SimpleTextResponder();
public delegate string SimpleTextResponderArgs(List<string> args);
public delegate string ComplexTextResponder(HttpRequest request, HttpResponse response);
```

Under the hood, each of these are wrapped by a `Responder` that sets the body of the `HttpResponse` to the string output of the simplified responder, sets content type to a separately provided value, and sets headers.

## **HttpMethod**

```C#
public enum HttpMethod {
    Get,
    Head,
    Post,
    Put,
    Delete,
    Patch
}
```

Asypi also contains a few convenience items for working with `HttpMethod`s:

`string.ToHttpMethod()`

`HttpMethod.AsString()`

## **HttpRequest**

`HttpRequest` is a wrapper over `System.Net.HttpListenerRequest`, exposing relevant fields.

### Public Fields

#### `string BodyText`

Gets the body of the request, as a `string`.

---

#### `string BodyBytes`

Gets the body of the request, as a `byte[]`.

### Other Public Fields

The majority of the fields in `System.Net.HttpListenerRequest` are directly wrapped by `HttpRequest`. For more information on these fields, refer to [Microsoft's official documentation](https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistenerrequest?view=net-5.0).

## **HttpResponse**

`HttpResponse` is a wrapper over `System.Net.HttpListenerResponse`, exposing relevant fields.

### Public Fields

#### `string BodyText`

Sets the body of the response, as a `string`.

---

#### `string BodyBytes`

Sets the body of the response, as a `byte[]`.

### Other Public Fields

The majority of the fields in `System.Net.HttpListenerResponse` are directly wrapped by `HttpResponse`. For more information on these fields, refer to [Microsoft's official documentation](https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistenerresponse?view=net-5.0).

## **IHeaders**

```C#
public interface IHeaders {
    Dictionary<string, string> Values { get; }
}
```

## **DefaultHeaders**

`DefaultHeaders` is an inheritable class implementing `IHeaders` containing sensible defaults for headers. To be precise, `DefaultHeaders` contains the following:

```text
Server: Asypi
X-Content-Type-Options: nosniff
"X-XSS-Protection: 1; mode=block
X-Frame-Options: SAMEORIGIN
Content-Security-Policy: script-src 'self'
```

## **FileServer**

`FileServer` is a static class provided by Asypi. It utilizes an LRU cache, which is set up when `Server` is initialized.

### Public Methods

#### `byte[] Get(string filePath)`

Gets the content of the file at `filePath` as a byte[], and then updates the LRU cache.

### `byte[] Read(string filePath)`

Gets the content of the file at `filePath` as a byte[], but does NOT update the LRU cache.
