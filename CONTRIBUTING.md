# Contributing

Asypi is currently set up under a BDFL system, with @therealstork at the helm. So long as you follow the following guidelines, any PRs you submit should be merged without issue:

NOTE: If you think any of the following guidelines are unreasonable, please open an issue on GitHub. Everything here is subject to change in light of new developments and/or reasoning.

## Formatting

@therealstork is very stubborn when it comes to his non-standard formatting. If a significant portion of changes start to come from people who would prefer normal C# formatting, however, this can change.

## API

The user-facing API should be as simple as possible while hiding as little as possible. For example, with the following function:

```C#
server.Route(
    HttpMethod.Get,
    "/",
    () => { return "Hello World!" },
    "text/html"
);

```

A user can reasonably infer that the server is registering a new route for GET requests to `/`, and will respond to these requests with the text in the provided function and signal the content type as `"text/html"`.

This is the desired type of API design. Avoid hiding things behind things like compile-time code generation (and, if possible, atttributes), where a user is forced to guess if and how their code will be transformed behind the scenes.

## Code Quality and Comments

Try to maintain the current quality and frequency of comments. Code that is difficult to understand even with comments should be rewritten unless doing so would result in significant performance regressions.

## Git

One commit per thing, and one thing per commit (consider a thing to be any reasonably significant and distinct item; e.g. a feature, bugfix, or enhancement). This ensures that the commit history is easily understandable, and one knows exactly what features they are including/excluding when performing `git checkout`.

Please use `git pull --rebase` to avoid ugly merge commits.
