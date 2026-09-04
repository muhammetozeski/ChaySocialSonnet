using ChaySocialSonnet.Web.Backend;

namespace ChaySocialSonnet.MainProject.Tests.Backend
{
    public class LocalMessageRelayTests
    {
        [Fact]
        public async Task GetInbox_ReturnsOnlyMessagesForThatRecipient()
        {
            var relay = new LocalMessageRelay();
            await relay.SendAsync("sender-1", "recipient-a", [1], [10]);
            await relay.SendAsync("sender-1", "recipient-b", [2], [20]);

            var inboxA = await relay.GetInboxAsync("recipient-a");

            Assert.Single(inboxA);
            Assert.Equal("sender-1", inboxA[0].SenderPublicId);
            Assert.Equal([10], inboxA[0].Ciphertext);
        }

        [Fact]
        public async Task GetInbox_ForRecipientWithNoMessages_ReturnsEmpty()
        {
            var relay = new LocalMessageRelay();

            var inbox = await relay.GetInboxAsync("nobody-sent-to-me");

            Assert.Empty(inbox);
        }
    }
}
