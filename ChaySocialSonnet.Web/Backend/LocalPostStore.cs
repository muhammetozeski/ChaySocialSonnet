using ChaySocialSonnet.MainProject.Backend;
using System.Collections.Concurrent;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary>
    /// In-memory <see cref="IPostStore"/> used while the project runs against the developer's own PC
    /// as its server. Posts are lost on restart — swap in a persistent implementation before this
    /// matters for real users.
    /// </summary>
    public sealed class LocalPostStore : IPostStore
    {
        readonly ConcurrentDictionary<string, PublicPost> posts = new();

        public Task<PublicPost> CreatePostAsync(string authorPublicId, string text)
        {
            var post = new PublicPost(Guid.NewGuid().ToString("n"), authorPublicId, text, DateTimeOffset.UtcNow);
            posts[post.Id] = post;
            return Task.FromResult(post);
        }

        public Task<IReadOnlyList<PublicPost>> GetRecentPostsAsync(int count)
        {
            IReadOnlyList<PublicPost> recentPosts = posts.Values
                .OrderByDescending(post => post.CreatedAt)
                .Take(count)
                .ToList();
            return Task.FromResult(recentPosts);
        }

        public Task<PublicPost?> GetByIdAsync(string postId) =>
            Task.FromResult(posts.TryGetValue(postId, out PublicPost? post) ? post : null);

        public Task<IReadOnlyList<PublicPost>> GetPostsByAuthorAsync(string authorPublicId, int count)
        {
            IReadOnlyList<PublicPost> authorPosts = posts.Values
                .Where(post => post.AuthorPublicId == authorPublicId)
                .OrderByDescending(post => post.CreatedAt)
                .Take(count)
                .ToList();
            return Task.FromResult(authorPosts);
        }

        public Task<bool> DeletePostAsync(string postId, string requestingPublicId)
        {
            bool deleted = posts.TryGetValue(postId, out PublicPost? post)
                && post.AuthorPublicId == requestingPublicId
                && posts.TryRemove(postId, out _);
            return Task.FromResult(deleted);
        }
    }
}
