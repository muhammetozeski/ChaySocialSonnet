namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary> A comment on a public post. Stored and served in the clear, same reasoning as <see cref="PublicPost"/>. </summary>
    public sealed record PostComment(string Id, string PostId, string AuthorPublicId, string Text, DateTimeOffset CreatedAt);

    /// <summary> Server-side storage for post comments. </summary>
    public interface ICommentStore
    {
        Task<PostComment> AddCommentAsync(string postId, string authorPublicId, string text);

        /// <summary> Comments on <paramref name="postId"/>, oldest first. </summary>
        Task<IReadOnlyList<PostComment>> GetCommentsAsync(string postId);

        Task<int> GetCommentCountAsync(string postId);
    }
}
