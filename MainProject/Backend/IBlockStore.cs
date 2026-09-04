namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary> Server-side storage for user-to-user blocks. A block is one-directional: only the blocker stops seeing/being reachable by the blocked user. </summary>
    public interface IBlockStore
    {
        Task BlockAsync(string blockerPublicId, string blockedPublicId);

        Task UnblockAsync(string blockerPublicId, string blockedPublicId);

        Task<bool> IsBlockedAsync(string blockerPublicId, string blockedPublicId);
    }
}
