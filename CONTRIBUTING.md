# Contributing

Asypi is currently set up under a BDFL system, with @therealstork at the helm. So long as you roughly follow the following rules, any PRs you submit should be merged without issue:

NOTE: If you think any of the following are unreasonable, please open an issue on GitHub. Everything here is subject to change in light of new developments and/or reasoning.

## Formatting

@therealstork is very stubborn when it comes to his non-standard formatting. If a significant portion of changes start to come from people who would prefer normal C# formatting, however, this can change.

## API

The user-facing API should be as simple as possible while hiding as little as possible. For example, with the following function:

```C#
server.AddRoute(
    HttpMethod.Get,
    "/",
    () => { return "Hello World!" },
    "text/html"
);

```

A user can reasonably infer that the server is registering a new route for GET requests to `/`, and will respond to these requests with the text in the provided function and signal the content type as `"text/html"`.

This is the desired type of API design. Avoid hiding things behind things like compile-time code generation (and, if possible, atttributes), where a user is forced to guess if and how their code will be transformed behind the scenes.

## Code Quality and Comments

The codebase as-is doesn't do a great job with comments, so the bar isn't very high for contributors. Unless if its truly difficult to understand what code does by reading it, feel free to call it self-documenting. As the project progresses, the expected quality of comments will hopefully increase.
