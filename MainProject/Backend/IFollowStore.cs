namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary> Server-side storage for the follow graph (asymmetric: following someone needs no approval). </summary>
    public interface IFollowStore
    {
        /// <summary> No-op if already following. </summary>
        Task FollowAsync(string followerPublicId, string targetPublicId);

        /// <summary> No-op if not currently following. </summary>
        Task UnfollowAsync(string followerPublicId, string targetPublicId);

        Task<bool> IsFollowingAsync(string followerPublicId, string targetPublicId);

        Task<int> GetFollowerCountAsync(string publicId);

        Task<int> GetFollowingCountAsync(string publicId);
    }
}
