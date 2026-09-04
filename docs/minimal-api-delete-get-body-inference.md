# Minimal APIs: MapDelete/MapGet reject an inferred request body

## Problem

Several `/api/*` endpoints need to read both a route parameter and a small JSON body — for example
`DELETE /api/posts/{postId}` needs `postId` from the route and `requestingPublicId` from the body to
check ownership before deleting. The natural minimal-API lambda looks the same as any `MapPost` handler:

```csharp
app.MapDelete("/api/posts/{postId}", async (string postId, DeletePostRequest request, IPostStore posts) =>
    await posts.DeletePostAsync(postId, request.RequestingPublicId) ? Results.Ok() : Results.NotFound());
```

## Symptom

`dotnet build` succeeds with zero errors. The app crashes the moment it actually starts
(`dotnet run` / a `preview_start` dev server), before it serves a single request:

```
crit: Microsoft.AspNetCore.Hosting.Diagnostics[6]
      Application startup exception
      System.InvalidOperationException: Body was inferred but the method does not allow inferred body parameters.
      Below is the list of parameters that we found:

      Parameter           | Source
      ---------------------------------------------------------------------------------
      postId              | Route (Inferred)
      request             | Body (Inferred)
      posts               | Services (Inferred)
```

Because this throws while ASP.NET Core is building the endpoint metadata cache (during
`CompositeEndpointDataSource.EnsureEndpointsInitialized`), it takes the *entire* app down, not just
that one route — every endpoint stops responding.

## Root cause

Minimal APIs infer where each parameter comes from (route, query, services, or body) by convention.
For `MapPost`/`MapPut`, a complex-type parameter with no matching route/query name is inferred as the
body. For `MapGet`/`MapDelete` (and `MapHead`), the framework deliberately refuses to make that same
inference — these verbs aren't conventionally expected to carry a body — so it throws instead of
silently guessing wrong. `dotnet build` can't catch this because it's a *runtime* check that only runs
when the endpoint route builder actually evaluates the lambda's parameters at startup.

## Solution

Mark the body parameter explicitly with `[FromBody]` on any `MapDelete`/`MapGet` handler that needs one:

```csharp
using Microsoft.AspNetCore.Mvc;

app.MapDelete("/api/posts/{postId}", async (string postId, [FromBody] DeletePostRequest request, IPostStore posts) =>
    await posts.DeletePostAsync(postId, request.RequestingPublicId) ? Results.Ok() : Results.NotFound());
```

`MapPost`/`MapPut` handlers with the same shape don't need this — only `MapDelete`/`MapGet`/`MapHead`.
The general lesson: **always actually start the app (not just `dotnet build`) after touching minimal-API
endpoint registrations** — this class of error is invisible until the host boots.
