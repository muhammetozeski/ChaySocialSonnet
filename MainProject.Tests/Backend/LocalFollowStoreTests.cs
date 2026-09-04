using ChaySocialSonnet.Web.Backend;

namespace ChaySocialSonnet.MainProject.Tests.Backend
{
    public class LocalFollowStoreTests
    {
        [Fact]
        public async Task FollowAsync_ThenIsFollowingAsync_ReturnsTrue()
        {
            var store = new LocalFollowStore();

            await store.FollowAsync("alice", "bob");

            Assert.True(await store.IsFollowingAsync("alice", "bob"));
        }

        [Fact]
        public async Task IsFollowingAsync_WithoutFollowing_ReturnsFalse()
        {
            var store = new LocalFollowStore();

            Assert.False(await store.IsFollowingAsync("alice", "bob"));
        }

        [Fact]
        public async Task UnfollowAsync_AfterFollowing_ReturnsFalse()
        {
            var store = new LocalFollowStore();
            await store.FollowAsync("alice", "bob");

            await store.UnfollowAsync("alice", "bob");

            Assert.False(await store.IsFollowingAsync("alice", "bob"));
        }

        [Fact]
        public async Task UnfollowAsync_WithoutEverFollowing_IsANoOp()
        {
            var store = new LocalFollowStore();

            await store.UnfollowAsync("alice", "bob");

            Assert.False(await store.IsFollowingAsync("alice", "bob"));
        }

        [Fact]
        public async Task FollowerAndFollowingCounts_ReflectBothSidesOfTheRelationship()
        {
            var store = new LocalFollowStore();
            await store.FollowAsync("alice", "bob");
            await store.FollowAsync("carol", "bob");

            Assert.Equal(2, await store.GetFollowerCountAsync("bob"));
            Assert.Equal(1, await store.GetFollowingCountAsync("alice"));
            Assert.Equal(0, await store.GetFollowerCountAsync("alice"));
        }

        [Fact]
        public async Task GetFollowingIdsAsync_ReturnsEveryoneFollowed()
        {
            var store = new LocalFollowStore();
            await store.FollowAsync("alice", "bob");
            await store.FollowAsync("alice", "carol");

            IReadOnlyList<string> following = await store.GetFollowingIdsAsync("alice");

            Assert.Equal(2, following.Count);
            Assert.Contains("bob", following);
            Assert.Contains("carol", following);
        }

        [Fact]
        public async Task GetFollowingIdsAsync_ForSomeoneWhoFollowsNobody_ReturnsEmpty()
        {
            var store = new LocalFollowStore();

            IReadOnlyList<string> following = await store.GetFollowingIdsAsync("alice");

            Assert.Empty(following);
        }
    }
}
