namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary> A post enriched with the like/comment stats a feed needs to render — what the server actually returns, as opposed to the bare <see cref="PublicPost"/> it stores. </summary>
    public sealed record PostSummary(string Id, string AuthorPublicId, string Text, DateTimeOffset CreatedAt, int LikeCount, int CommentCount, bool LikedByViewer);

    public sealed record CreatePostRequest(string AuthorPublicId, string Text);

    public sealed record ToggleLikeRequest(string LikerPublicId);

    public sealed record ToggleLikeResponse(bool Liked, int LikeCount);

    public sealed record AddCommentRequest(string AuthorPublicId, string Text);

    public sealed record CommentResponse(string Id, string AuthorPublicId, string Text, DateTimeOffset CreatedAt);

    public sealed record DeletePostRequest(string RequestingPublicId);
}
