using ChaySocialSonnet.MainProject.Backend;
using System.Collections.Concurrent;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary> In-memory <see cref="ICommentStore"/>. Lost on restart, same as the other Local* stores. </summary>
    public sealed class LocalCommentStore : ICommentStore
    {
        readonly ConcurrentDictionary<string, ConcurrentQueue<PostComment>> commentsByPost = new();

        public Task<PostComment> AddCommentAsync(string postId, string authorPublicId, string text)
        {
            var comment = new PostComment(Guid.NewGuid().ToString("n"), postId, authorPublicId, text, DateTimeOffset.UtcNow);
            ConcurrentQueue<PostComment> comments = commentsByPost.GetOrAdd(postId, static _ => new ConcurrentQueue<PostComment>());
            comments.Enqueue(comment);
            return Task.FromResult(comment);
        }

        public Task<IReadOnlyList<PostComment>> GetCommentsAsync(string postId)
        {
            IReadOnlyList<PostComment> comments = commentsByPost.TryGetValue(postId, out ConcurrentQueue<PostComment>? queue)
                ? queue.ToArray()
                : [];
            return Task.FromResult(comments);
        }

        public Task<int> GetCommentCountAsync(string postId) =>
            Task.FromResult(commentsByPost.TryGetValue(postId, out ConcurrentQueue<PostComment>? queue) ? queue.Count : 0);
    }
}
