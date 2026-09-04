using ChaySocialSonnet.Web.Backend;

namespace ChaySocialSonnet.MainProject.Tests.Backend
{
    public class LocalPostStoreTests
    {
        [Fact]
        public async Task GetRecentPosts_ReturnsNewestFirst()
        {
            var store = new LocalPostStore();
            var first = await store.CreatePostAsync("author-1", "first post");
            await Task.Delay(5);
            var second = await store.CreatePostAsync("author-1", "second post");

            var recentPosts = await store.GetRecentPostsAsync(10);

            Assert.Equal([second.Id, first.Id], recentPosts.Select(post => post.Id));
        }

        [Fact]
        public async Task GetRecentPosts_RespectsRequestedCount()
        {
            var store = new LocalPostStore();
            for (var i = 0; i < 5; i++)
            {
                await store.CreatePostAsync("author-1", $"post {i}");
            }

            var recentPosts = await store.GetRecentPostsAsync(2);

            Assert.Equal(2, recentPosts.Count);
        }
    }
}
