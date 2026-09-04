# ChaySocial

![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![C%23](https://img.shields.io/badge/C%23-14-239120)
![Blazor](https://img.shields.io/badge/Blazor-Web%20App-5C2D91)
![Status](https://img.shields.io/badge/status-active%20development-yellow)

A social media app built from scratch as a Blazor Web App (ASP.NET Core server + WebAssembly client),
with a MAUI Blazor Hybrid shell in the repository for a future native build. There is no traditional
sign-up: an account is a post-quantum keypair generated on-device, and the private key never leaves it.

This project is under continuous, active development. Data is currently stored in-memory on the server
process (a real database is a planned, isolated later step — see [Architecture](#architecture)).

## Features

- **One-tap accounts, no passwords.** Creating an account generates an ML-DSA (signing) and ML-KEM
  (key exchange) keypair on-device. The public identity is a short id derived from the public key; the
  private key is saved locally (browser storage on web, platform secure storage on MAUI) and is never
  sent to the server.
- **Challenge-response sign-in.** Signing back in proves ownership of the private key by signing a
  server-issued nonce — the private key itself is never transmitted. A successful sign-in gets a session
  token, and every action that changes something server-side (posting, liking, following, blocking...)
  must present that token; the server never trusts a public id a client just types into a request.
- **Identity backup and restore.** A passphrase-encrypted backup file can be downloaded from the profile
  page and restored from the login page on any device — losing this device's local storage doesn't have
  to mean losing the account, and the passphrase/private key never reach the server.
- **Public feed.** A composer for short text posts, a For You / Following feed toggle, likes, and threaded
  comments, plus a direct link to any single post. Public posts are stored and served in the clear by
  design — they are meant to be visible to everyone, so encrypting them would not add any real
  confidentiality.
- **End-to-end encrypted direct messages.** A message is encrypted client-side with AES-256-GCM under a
  key derived from an ML-KEM key exchange with the recipient. The server only ever relays opaque
  ciphertext — it cannot read message content.
- **Following, search, notifications.** Follow/unfollow with follower and following counts, search by
  display name or identity id, and a notification feed for likes, comments, and new followers, each
  linking to the relevant post or profile.
- **Blocking and reporting.** Block another identity — their posts and comments stop appearing for each
  other on both sides — or report a post or profile for later review.
- **Own profile.** Shows your identity (with a one-tap copy button), your posts, and your follow counts.

## Security architecture

Every cryptographic primitive here — the post-quantum ones (ML-DSA, ML-KEM), the symmetric cipher for
messages and backups (AES-256-GCM), and the passphrase key derivation (PBKDF2) — is implemented with
[BouncyCastle](https://www.bouncycastle.org/), entirely in managed code, rather than the .NET runtime's
own cryptography APIs. This is deliberate, not a style preference: the same code has to run both in the
browser (Blazor WebAssembly) and in the native MAUI app, and several of .NET's own crypto APIs turned out
not to work under WebAssembly during development — `System.Security.Cryptography.Rfc2898DeriveBytes`, for
instance, hangs indefinitely there instead of throwing or returning.

| Purpose | Algorithm |
|---|---|
| Account identity / signing | ML-DSA-65 |
| Key exchange for messages | ML-KEM-768 |
| Message and backup content encryption | AES-256-GCM |
| Backup passphrase key derivation | PBKDF2-HMAC-SHA256 (200,000 iterations) |

The server-side identity registry enforces that an account's public id is always the hash of its own
signing public key, and rejects re-registering an existing id under a different key — this is the
system's account-takeover protection, and it lives in the registry itself so every future implementation
(including a database-backed one) is required to enforce it, not just the current in-memory one.

## Architecture

The project is split so that swapping the storage backend (in-memory today, a real database or Firebase
later) is a dependency-injection change, not a change to any calling code:

- **`MainProject`** — a Razor Class Library shared between the web app and the MAUI app: UI components,
  pages, theming, client-side services (identity/crypto, API clients), and the backend *interfaces*
  (`IPostStore`, `IIdentityRegistry`, `IFollowStore`, etc.) with no implementation.
- **`ChaySocialSonnet.Web`** — the ASP.NET Core server host. Registers the current in-memory (`Local*`)
  implementations of those interfaces and maps the `/api/*` minimal API endpoints.
- **`ChaySocialSonnet.Web.Client`** — the Blazor WebAssembly client project that runs the shared UI in
  the browser.
- **`ChaySocialSonnet`** — the .NET MAUI Blazor Hybrid app shell (Android/Windows/iOS/MacCatalyst),
  reusing the same `MainProject` UI and services for a future native build.
- **`MainProject.Tests`** — xUnit tests, mainly covering the identity/crypto round trips and the
  in-memory stores.

## Getting started

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```powershell
dotnet run --project ChaySocialSonnet.Web/ChaySocialSonnet.Web.csproj
```

Then open the URL printed in the console (`http://localhost:5017` by default). All data lives in the
server process's memory and is lost on restart.

To run the test suite:

```powershell
dotnet test MainProject.Tests/MainProject.Tests.csproj
```

The MAUI app (`ChaySocialSonnet.csproj`) requires the .NET MAUI workload and platform-specific tooling
(Android SDK, etc.) to build, and does not yet have a server address configured for talking to a
deployed backend.
