# Middleware

Asypi supports a versatile middleware system, allowing one to view, modify, or cancel request/response pairs before a responder ever sees them.

To understand how middleware in Asypi works, first the "response chain" must be understood. When Asypi receives an HTTP request, a request/response pair is created and then sent as followed: `server -> router -> middleware -> responder`.

Middleware has the power to "break" the response chain, preventing successive middleware or the responder from operating on the request/response pair.

## Writing Middleware

In Asypi, middleware is simply a function with the following signature:

```C#
(Req req, Res res) => {
    // do something
    return shouldContinue;
}
```

If a middleware function returns false, it will "break the response chain," preventing successive middleware or the responder from operating on the request/response pair. If a middleware function returns true, the request/response pair will be passed down the chain normally.

## Using Middleware

Middleware can be used with a simple call to `Server.Use()`. For example, to register `doStuff()` as middleware for all routes, one would call `Server.Use(".*", doStuff);`.

## Middleware Example

```C#
// on all routes that start with private/
server.use(@"private\/.*", (Req req, Res res) => {
    // don't load the page if user is not authenticated
    if (isAuthenticated(req)) {
        return true;
    } else {
        return false;
    }
});
```
