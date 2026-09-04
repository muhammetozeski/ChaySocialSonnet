using ChaySocialSonnet.Web.Backend;

namespace ChaySocialSonnet.MainProject.Tests.Backend
{
    public class LocalLikeStoreTests
    {
        [Fact]
        public async Task ToggleLikeAsync_FirstCall_Likes()
        {
            var store = new LocalLikeStore();

            var liked = await store.ToggleLikeAsync("post1", "alice");

            Assert.True(liked);
            Assert.True(await store.HasLikedAsync("post1", "alice"));
            Assert.Equal(1, await store.GetLikeCountAsync("post1"));
        }

        [Fact]
        public async Task ToggleLikeAsync_SecondCall_Unlikes()
        {
            var store = new LocalLikeStore();
            await store.ToggleLikeAsync("post1", "alice");

            var liked = await store.ToggleLikeAsync("post1", "alice");

            Assert.False(liked);
            Assert.False(await store.HasLikedAsync("post1", "alice"));
            Assert.Equal(0, await store.GetLikeCountAsync("post1"));
        }

        [Fact]
        public async Task ToggleLikeAsync_DifferentLikers_CountEachIndependently()
        {
            var store = new LocalLikeStore();
            await store.ToggleLikeAsync("post1", "alice");
            await store.ToggleLikeAsync("post1", "bob");

            Assert.Equal(2, await store.GetLikeCountAsync("post1"));
        }

        [Fact]
        public async Task ToggleLikeAsync_ManyConcurrentTogglesFromSameLiker_NeverReportsStateDisagreeingWithActualState()
        {
            var store = new LocalLikeStore();
            const int concurrentCalls = 40;

            var tasks = Enumerable.Range(0, concurrentCalls)
                .Select(_ => store.ToggleLikeAsync("post1", "alice"))
                .ToArray();
            bool[] results = await Task.WhenAll(tasks);

            bool finalState = await store.HasLikedAsync("post1", "alice");
            int trueCount = results.Count(r => r);
            int falseCount = results.Count(r => !r);

            // Every call flips the state once, so with an EVEN number of calls the net effect is "back to
            // unliked" and an ODD number ends "liked" — the whole point of the atomicity fix is that this
            // holds exactly, with no call's reported result disagreeing with the final persisted state.
            Assert.Equal(concurrentCalls % 2 == 1, finalState);
            Assert.Equal(concurrentCalls, trueCount + falseCount);
            // The number of "liked" results must be odd if final state is liked, even otherwise — i.e.
            // trueCount and finalState must have matching parity; this is what a lost toggle would violate.
            Assert.Equal(finalState, trueCount % 2 == 1);
        }
    }
}
