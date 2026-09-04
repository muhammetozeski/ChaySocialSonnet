using ChaySocialSonnet.MainProject.Backend;
using System.Collections.Concurrent;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> In-memory <see cref="ILikeStore"/>. Lost on restart, same as the other Local* stores. </summary>
    public sealed class LocalLikeStore : ILikeStore
    {
        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> likersByPost = new();

        public Task<bool> ToggleLikeAsync(string postId, string likerPublicId)
        {
            ConcurrentDictionary<string, byte> likers = likersByPost.GetOrAdd(postId, static _ => new ConcurrentDictionary<string, byte>());

            // TryRemove-then-TryAdd is each individually atomic, but the PAIR isn't: two overlapping toggles
            // from the same liker could both miss the TryRemove and then race on TryAdd, leaving the loser
            // reporting the opposite of the state it actually caused. Locking on this post's own likers
            // dictionary (never exposed outside this class) serializes toggles for that one post only.
            lock (likers)
            {
                bool nowLiked = !likers.TryRemove(likerPublicId, out _) && likers.TryAdd(likerPublicId, 0);
                return Task.FromResult(nowLiked);
            }
        }

        public Task<int> GetLikeCountAsync(string postId) =>
            Task.FromResult(likersByPost.TryGetValue(postId, out ConcurrentDictionary<string, byte>? likers) ? likers.Count : 0);

        public Task<bool> HasLikedAsync(string postId, string likerPublicId) =>
            Task.FromResult(likersByPost.TryGetValue(postId, out ConcurrentDictionary<string, byte>? likers) && likers.ContainsKey(likerPublicId));
    }
}
