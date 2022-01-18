# Introduction

A simple, asynchronous, and flexible web framework built on `System.Net.HttpListener`. Pronounced "AS-eh-PIE."

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
