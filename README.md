# Asypi

A simple, asynchronous, and scalable HTTP server built on `System.Net.HttpListener`. Pronounced "AS-eh-PIE."

## Hello World Example

```C#
using Asypi;

Server server = new Server(8000, "localhost");

server.Route(
    HttpMethod.Get,
    "/",
    () => { return "Hello World!"; },
    "text/html"
);

server.Run();

```

## Features

- Simple declarative API
- Async support
- Flexible router supporting variable parameters
- Cached static file server supporting custom mount points

## Use Cases

### Asypi is

- Simple, asynchronous, and scalable
- Good for proof-of-concept projects
- Good for simple websites
- Good for simple REST APIs

### Asypi is **not**

- **Stable.** Do not use Asypi if you need stability.
- **Well-Tested.** Do not use Asypi for mission-critical or security-critical applications.
- Suitable for production usage unless if you *really* know what you're doing.
- Good for complex projects. For complex needs, consider alternatives such as ASP.NET.

Hopefully, as the project progresses, the first 3 of these caveats can be removed.

## Contributing

If you like this project and want to contribute, please come and do so! More information in [CONTRIBUTING.md](./CONTRIBUTING.md)

## Documentation

Asypi is currently in the very early stages of development, and as a result has no proper documentation. Use at your own risk.
