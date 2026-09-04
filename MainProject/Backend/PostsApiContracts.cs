namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary> A post enriched with the like/comment stats a feed needs to render — what the server actually returns, as opposed to the bare <see cref="PublicPost"/> it stores. </summary>
    public sealed record PostSummary(string Id, string AuthorPublicId, string Text, DateTimeOffset CreatedAt, int LikeCount, int CommentCount, bool LikedByViewer);

    /// <summary> The author is the caller resolved server-side from their session token — never a field in this request, so a client can't post as anyone but itself. </summary>
    public sealed record CreatePostRequest(string Text);

    public sealed record ToggleLikeResponse(bool Liked, int LikeCount);

    /// <summary> The author is the caller resolved server-side from their session token — never a field in this request. </summary>
    public sealed record AddCommentRequest(string Text);

    public sealed record CommentResponse(string Id, string AuthorPublicId, string Text, DateTimeOffset CreatedAt);
}
