using ChaySocialSonnet.MainProject.Backend;
using System.Collections.Concurrent;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> In-memory <see cref="IBlockStore"/>. Lost on restart, same as the other Local* stores. </summary>
    public sealed class LocalBlockStore : IBlockStore
    {
        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> blockedByBlocker = new();

        public Task BlockAsync(string blockerPublicId, string blockedPublicId)
        {
            blockedByBlocker.GetOrAdd(blockerPublicId, static _ => new ConcurrentDictionary<string, byte>())[blockedPublicId] = 0;
            return Task.CompletedTask;
        }

        public Task UnblockAsync(string blockerPublicId, string blockedPublicId)
        {
            if (blockedByBlocker.TryGetValue(blockerPublicId, out ConcurrentDictionary<string, byte>? blocked))
            {
                blocked.TryRemove(blockedPublicId, out _);
            }
            return Task.CompletedTask;
        }

        public Task<bool> IsBlockedAsync(string blockerPublicId, string blockedPublicId) =>
            Task.FromResult(blockedByBlocker.TryGetValue(blockerPublicId, out ConcurrentDictionary<string, byte>? blocked) && blocked.ContainsKey(blockedPublicId));
    }
}
