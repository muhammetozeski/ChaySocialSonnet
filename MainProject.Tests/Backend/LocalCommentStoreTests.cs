using ChaySocialSonnet.MainProject.Backend;
using ChaySocialSonnet.Web.Backend;

namespace ChaySocialSonnet.MainProject.Tests.Backend
{
    public class LocalCommentStoreTests
    {
        [Fact]
        public async Task AddCommentAsync_ThenGetCommentsAsync_ReturnsIt()
        {
            var store = new LocalCommentStore();

            PostComment added = await store.AddCommentAsync("post1", "alice", "nice post!");
            IReadOnlyList<PostComment> comments = await store.GetCommentsAsync("post1");

            PostComment found = Assert.Single(comments);
            Assert.Equal(added.Id, found.Id);
            Assert.Equal("alice", found.AuthorPublicId);
            Assert.Equal("nice post!", found.Text);
        }

        [Fact]
        public async Task GetCommentsAsync_ForPostWithNoComments_ReturnsEmpty()
        {
            var store = new LocalCommentStore();

            IReadOnlyList<PostComment> comments = await store.GetCommentsAsync("post1");

            Assert.Empty(comments);
        }

        [Fact]
        public async Task GetCommentsAsync_ReturnsOldestFirst()
        {
            var store = new LocalCommentStore();
            await store.AddCommentAsync("post1", "alice", "first");
            await store.AddCommentAsync("post1", "bob", "second");

            IReadOnlyList<PostComment> comments = await store.GetCommentsAsync("post1");

            Assert.Equal("first", comments[0].Text);
            Assert.Equal("second", comments[1].Text);
        }

        [Fact]
        public async Task GetCommentCountAsync_MatchesNumberOfCommentsAdded()
        {
            var store = new LocalCommentStore();
            await store.AddCommentAsync("post1", "alice", "one");
            await store.AddCommentAsync("post1", "bob", "two");

            Assert.Equal(2, await store.GetCommentCountAsync("post1"));
        }

        [Fact]
        public async Task Comments_OnDifferentPosts_DoNotMix()
        {
            var store = new LocalCommentStore();
            await store.AddCommentAsync("post1", "alice", "on post1");
            await store.AddCommentAsync("post2", "bob", "on post2");

            IReadOnlyList<PostComment> post1Comments = await store.GetCommentsAsync("post1");

            PostComment found = Assert.Single(post1Comments);
            Assert.Equal("on post1", found.Text);
        }
    }
}
