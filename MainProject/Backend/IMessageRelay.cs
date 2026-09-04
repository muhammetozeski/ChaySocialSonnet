namespace ChaySocialSonnet.MainProject.Backend
{
    /// <summary>
    /// One end-to-end encrypted direct message as the server sees it: an ML-KEM encapsulated shared
    /// secret plus the AES-encrypted ciphertext. The server relays and stores these bytes only — it
    /// never holds a private key that could decrypt <see cref="Ciphertext"/>.
    /// </summary>
    public sealed record EncryptedMessage(string SenderPublicId, byte[] EncapsulatedKey, byte[] Ciphertext, DateTimeOffset SentAt);

    /// <summary>
    /// Server-side relay/storage for end-to-end encrypted direct messages. Implemented locally for now
    /// (<c>ChaySocialSonnet.Web</c>'s in-memory relay); swapping in a Firebase-backed implementation
    /// later means registering a different <see cref="IMessageRelay"/> in DI, nothing else.
    /// </summary>
    public interface IMessageRelay
    {
        /// <summary> Stores an already-encrypted message for later delivery to <paramref name="recipientPublicId"/>. </summary>
        Task SendAsync(string senderPublicId, string recipientPublicId, byte[] encapsulatedKey, byte[] ciphertext);

        /// <summary> Returns every encrypted message waiting for <paramref name="recipientPublicId"/>, oldest first. </summary>
        Task<IReadOnlyList<EncryptedMessage>> GetInboxAsync(string recipientPublicId);
    }
}
