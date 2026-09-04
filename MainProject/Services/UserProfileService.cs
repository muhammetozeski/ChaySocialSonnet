using ChaySocialSonnet.MainProject.DataModels;
using ChaySocialSonnet.MainProject.Events;

namespace ChaySocialSonnet.MainProject.Services
{
    public static class UserProfileService
    {
        public static UserProfileData? LoggedUserProfileData { get; set; }
        public static readonly AppEvent OnProfileLoaded = new();
        public static readonly AppEvent OnProfileUpdated = new();

        public static async Task<UserProfileData?> GetProfileAsync(string userId)
        {
            // Add your generic get profile logic here
            await Task.CompletedTask;
            return LoggedUserProfileData;
        }

        public static async Task UpdateProfileAsync(UserProfileData profileData)
        {
            // Add your generic update profile logic here
            LoggedUserProfileData = profileData;
            await Task.CompletedTask;
            OnProfileUpdated.Raise();
        }

        public static async Task LoadProfileAsync()
        {
            // Add your load logic here
            await Task.CompletedTask;
            OnProfileLoaded.Raise();
        }
    }
}
