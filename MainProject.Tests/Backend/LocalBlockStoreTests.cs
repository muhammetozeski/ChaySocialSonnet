using ChaySocialSonnet.Web.Backend;

namespace ChaySocialSonnet.MainProject.Tests.Backend
{
    public class LocalBlockStoreTests
    {
        [Fact]
        public async Task BlockAsync_ThenIsBlockedAsync_ReturnsTrue()
        {
            var store = new LocalBlockStore();

            await store.BlockAsync("alice", "bob");

            Assert.True(await store.IsBlockedAsync("alice", "bob"));
        }

        [Fact]
        public async Task IsBlockedAsync_WithoutBlocking_ReturnsFalse()
        {
            var store = new LocalBlockStore();

            Assert.False(await store.IsBlockedAsync("alice", "bob"));
        }

        [Fact]
        public async Task Block_IsOneDirectional()
        {
            var store = new LocalBlockStore();

            await store.BlockAsync("alice", "bob");

            Assert.True(await store.IsBlockedAsync("alice", "bob"));
            Assert.False(await store.IsBlockedAsync("bob", "alice"));
        }

        [Fact]
        public async Task UnblockAsync_AfterBlocking_ReturnsFalse()
        {
            var store = new LocalBlockStore();
            await store.BlockAsync("alice", "bob");

            await store.UnblockAsync("alice", "bob");

            Assert.False(await store.IsBlockedAsync("alice", "bob"));
        }

        [Fact]
        public async Task UnblockAsync_WithoutEverBlocking_IsANoOp()
        {
            var store = new LocalBlockStore();

            await store.UnblockAsync("alice", "bob");

            Assert.False(await store.IsBlockedAsync("alice", "bob"));
        }
    }
}
