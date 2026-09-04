using ChaySocialSonnet.MainProject.Services.Identity;

namespace ChaySocialSonnet.MainProject.Services
{
    public static class AuthService
    {
        public static event Action? OnAuthStateChanged;
        public static bool IsLoggedIn { get; private set; }

        /// <summary> The signed-in identity for this app session (in-memory only — lost on reload until a persistent on-device key store is added). Null while signed out. </summary>
        public static ChayIdentity? CurrentIdentity { get; private set; }

        public static async Task InitAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary> Marks <paramref name="identity"/> as the signed-in identity for this session and raises <see cref="OnAuthStateChanged"/>. </summary>
        public static void SignIn(ChayIdentity identity)
        {
            CurrentIdentity = identity;
            IsLoggedIn = true;
            OnAuthStateChanged?.Invoke();
        }

        /// <summary> Clears the signed-in identity and raises <see cref="OnAuthStateChanged"/>. </summary>
        public static void SignOut()
        {
            CurrentIdentity = null;
            IsLoggedIn = false;
            OnAuthStateChanged?.Invoke();
        }
    }
}
