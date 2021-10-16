# Asypi

[![Documentation Status](https://readthedocs.org/projects/asypi/badge/?version=latest)](https://asypi.readthedocs.io/en/latest/?badge=latest)

A simple, asynchronous, and scalable HTTP server built on `System.Net.HttpListener`. Pronounced "AS-eh-PIE."

[Docs](https://asypi.readthedocs.io/en/latest/introduction.html)

[nuget](https://www.nuget.org/packages/Asypi/)

## Installation

`dotnet add package Asypi`

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

Hopefully, as the project progresses, these caveats can be removed.

## Contributing

If you like this project and want to contribute, please come and do so! More information in [CONTRIBUTING.md](./CONTRIBUTING.md)

## Documentation

Documentation for Asypi can be found on [readthedocs.io](https://asypi.readthedocs.io/en/latest/).
