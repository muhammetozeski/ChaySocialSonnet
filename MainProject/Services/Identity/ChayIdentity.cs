namespace ChaySocialSonnet.MainProject.Services.Identity
{
    /// <summary>
    /// A generated account identity: an ML-DSA signing keypair (proves who you are to the server
    /// without ever sending the private key) paired with an ML-KEM keypair (lets other users
    /// encapsulate a shared secret to you for end-to-end encrypted messaging). Only
    /// <see cref="PublicId"/>, <see cref="SigningPublicKey"/> and <see cref="EncryptionPublicKey"/>
    /// are ever meant to leave the device that generated this identity.
    /// </summary>
    /// <param name="PublicId">Short, shareable identity derived from a hash of <see cref="SigningPublicKey"/>.</param>
    /// <param name="SigningPublicKey">ML-DSA public key, verifies signatures made with <see cref="SigningPrivateKey"/>.</param>
    /// <param name="SigningPrivateKey">ML-DSA private key. Never transmitted anywhere.</param>
    /// <param name="EncryptionPublicKey">ML-KEM public key, lets others encapsulate a shared secret addressed to this identity.</param>
    /// <param name="EncryptionPrivateKey">ML-KEM private key. Never transmitted anywhere.</param>
    public sealed record ChayIdentity(
        string PublicId,
        byte[] SigningPublicKey,
        byte[] SigningPrivateKey,
        byte[] EncryptionPublicKey,
        byte[] EncryptionPrivateKey);
}
