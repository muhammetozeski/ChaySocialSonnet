namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary>
    /// A post on the public main wall. Stored and readable in the clear — the user decided public posts
    /// are not end-to-end encrypted, since content meant for every visitor cannot meaningfully be hidden
    /// from the one party (the server) that has to distribute it to everyone.
    /// </summary>
    public sealed record PublicPost(string Id, string AuthorPublicId, string Text, DateTimeOffset CreatedAt);

    /// <summary>
    /// Server-side storage for public wall posts. Implemented locally for now
    /// (<c>ChaySocialSonnet.Web</c>'s in-memory store); swapping in a Firebase-backed implementation
    /// later means registering a different <see cref="IPostStore"/> in DI, nothing else.
    /// </summary>
    public interface IPostStore
    {
        /// <summary> Creates a new public post authored by <paramref name="authorPublicId"/>. </summary>
        Task<PublicPost> CreatePostAsync(string authorPublicId, string text);

        /// <summary> Returns the most recent <paramref name="count"/> public posts, newest first. </summary>
        Task<IReadOnlyList<PublicPost>> GetRecentPostsAsync(int count);

        /// <summary> Looks up a single post by id, or null if it doesn't exist (or was deleted). </summary>
        Task<PublicPost?> GetByIdAsync(string postId);

        /// <summary> Returns the most recent <paramref name="count"/> posts by <paramref name="authorPublicId"/>, newest first. </summary>
        Task<IReadOnlyList<PublicPost>> GetPostsByAuthorAsync(string authorPublicId, int count);

        /// <summary> Deletes <paramref name="postId"/> if it exists and was authored by <paramref name="requestingPublicId"/>. Returns whether it was deleted. </summary>
        Task<bool> DeletePostAsync(string postId, string requestingPublicId);
    }
}
