using ChaySocialSonnet.MainProject.Events;
using ChaySocialSonnet.MainProject.Services.Identity;

namespace ChaySocialSonnet.MainProject.Services
{
    public static class AuthService
    {
        public static readonly AppEvent OnAuthStateChanged = new();
        public static bool IsLoggedIn { get; private set; }

        /// <summary> The signed-in identity for this app session (in-memory only — lost on reload until a persistent on-device key store is added). Null while signed out. </summary>
        public static ChayIdentity? CurrentIdentity { get; private set; }

        /// <summary>
        /// The bearer token proving control of <see cref="CurrentIdentity"/> to the server (from
        /// <c>IdentityApiClient.SignInAsync</c>'s challenge-response round trip). Every API client that
        /// mutates state on the caller's behalf must send this, not just <see cref="CurrentIdentity"/>'s
        /// public id — the server only trusts identity claims that come with a valid token.
        /// </summary>
        public static string? SessionToken { get; private set; }

        public static async Task InitAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary> Marks <paramref name="identity"/> as the signed-in identity for this session (with the session token proving it to the server) and raises <see cref="OnAuthStateChanged"/>. </summary>
        public static void SignIn(ChayIdentity identity, string sessionToken)
        {
            CurrentIdentity = identity;
            SessionToken = sessionToken;
            IsLoggedIn = true;
            OnAuthStateChanged.Raise();
        }

        /// <summary> Clears the signed-in identity and session token, and raises <see cref="OnAuthStateChanged"/>. </summary>
        public static void SignOut()
        {
            CurrentIdentity = null;
            SessionToken = null;
            IsLoggedIn = false;
            OnAuthStateChanged.Raise();
        }
    }
}
