using ChaySocialSonnet.MainProject.Backend;
using System.Collections.Concurrent;

namespace ChaySocialSonnet.Web.Backend
{
    /// <summary>
    /// In-memory <see cref="IMessageRelay"/> used while the project runs against the developer's own
    /// PC as its server. Queued messages are lost on restart — swap in a persistent implementation
    /// before this matters for real users.
    /// </summary>
    public sealed class LocalMessageRelay : IMessageRelay
    {
        readonly ConcurrentDictionary<string, ConcurrentQueue<EncryptedMessage>> inboxesByRecipient = new();

        public Task SendAsync(string senderPublicId, string recipientPublicId, byte[] encapsulatedKey, byte[] ciphertext)
        {
            ConcurrentQueue<EncryptedMessage> inbox = inboxesByRecipient.GetOrAdd(recipientPublicId, static _ => new ConcurrentQueue<EncryptedMessage>());
            inbox.Enqueue(new EncryptedMessage(senderPublicId, encapsulatedKey, ciphertext, DateTimeOffset.UtcNow));
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<EncryptedMessage>> GetInboxAsync(string recipientPublicId)
        {
            IReadOnlyList<EncryptedMessage> messages = inboxesByRecipient.TryGetValue(recipientPublicId, out ConcurrentQueue<EncryptedMessage>? inbox)
                ? inbox.ToArray()
                : [];
            return Task.FromResult(messages);
        }
    }
}
