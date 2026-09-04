using ChaySocialSonnet.MainProject.DataModels;

namespace ChaySocialSonnet.MainProject.Services
{
    public static class AuthService
    {
        public static event Action? OnAuthStateChanged;
        public static bool IsLoggedIn { get; set; }

        public static async Task InitAsync()
        {
            await Task.CompletedTask;
        }
    }
}
