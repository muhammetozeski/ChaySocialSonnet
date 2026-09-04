using ChaySocialSonnet.MainProject.Backend;
using System.Collections.Concurrent;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> In-memory <see cref="IFollowStore"/>. Lost on restart, same as the other Local* stores. </summary>
    public sealed class LocalFollowStore : IFollowStore
    {
        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> followingByFollower = new();
        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> followersByTarget = new();

        public Task FollowAsync(string followerPublicId, string targetPublicId)
        {
            followingByFollower.GetOrAdd(followerPublicId, static _ => new ConcurrentDictionary<string, byte>())[targetPublicId] = 0;
            followersByTarget.GetOrAdd(targetPublicId, static _ => new ConcurrentDictionary<string, byte>())[followerPublicId] = 0;
            return Task.CompletedTask;
        }

        public Task UnfollowAsync(string followerPublicId, string targetPublicId)
        {
            if (followingByFollower.TryGetValue(followerPublicId, out ConcurrentDictionary<string, byte>? following))
            {
                following.TryRemove(targetPublicId, out _);
            }
            if (followersByTarget.TryGetValue(targetPublicId, out ConcurrentDictionary<string, byte>? followers))
            {
                followers.TryRemove(followerPublicId, out _);
            }
            return Task.CompletedTask;
        }

        public Task<bool> IsFollowingAsync(string followerPublicId, string targetPublicId) =>
            Task.FromResult(followingByFollower.TryGetValue(followerPublicId, out ConcurrentDictionary<string, byte>? following) && following.ContainsKey(targetPublicId));

        public Task<int> GetFollowerCountAsync(string publicId) =>
            Task.FromResult(followersByTarget.TryGetValue(publicId, out ConcurrentDictionary<string, byte>? followers) ? followers.Count : 0);

        public Task<int> GetFollowingCountAsync(string publicId) =>
            Task.FromResult(followingByFollower.TryGetValue(publicId, out ConcurrentDictionary<string, byte>? following) ? following.Count : 0);

        public Task<IReadOnlyList<string>> GetFollowingIdsAsync(string publicId)
        {
            IReadOnlyList<string> ids = followingByFollower.TryGetValue(publicId, out ConcurrentDictionary<string, byte>? following)
                ? following.Keys.ToList()
                : [];
            return Task.FromResult(ids);
        }
    }
}
