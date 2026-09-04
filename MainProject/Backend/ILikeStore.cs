namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary> Server-side storage for post likes. </summary>
    public interface ILikeStore
    {
        /// <summary> Toggles whether <paramref name="likerPublicId"/> likes <paramref name="postId"/>. Returns the new liked state. </summary>
        Task<bool> ToggleLikeAsync(string postId, string likerPublicId);

        /// <summary> Total number of likes on <paramref name="postId"/>. </summary>
        Task<int> GetLikeCountAsync(string postId);

        /// <summary> Whether <paramref name="likerPublicId"/> currently likes <paramref name="postId"/>. </summary>
        Task<bool> HasLikedAsync(string postId, string likerPublicId);
    }
}
