using ChaySocialSonnet.MainProject.Services.Identity;

namespace ChaySocialSonnet.MainProject.Tests.Services.Identity
{
    public class IdentityServiceTests
    {
        [Fact]
        public void GenerateIdentity_ProducesNonEmptyKeysAndMatchingPublicId()
        {
            var identity = IdentityService.GenerateIdentity();

            Assert.NotEmpty(identity.SigningPublicKey);
            Assert.NotEmpty(identity.SigningPrivateKey);
            Assert.NotEmpty(identity.EncryptionPublicKey);
            Assert.NotEmpty(identity.EncryptionPrivateKey);
            Assert.Equal(IdentityService.DerivePublicId(identity.SigningPublicKey), identity.PublicId);
        }

        [Fact]
        public void GenerateIdentity_TwoCalls_ProduceDifferentIdentities()
        {
            var first = IdentityService.GenerateIdentity();
            var second = IdentityService.GenerateIdentity();

            Assert.NotEqual(first.PublicId, second.PublicId);
        }

        [Fact]
        public void Sign_ThenVerify_WithMatchingPublicKey_ReturnsTrue()
        {
            var identity = IdentityService.GenerateIdentity();
            var message = "prove-you-hold-the-private-key"u8.ToArray();

            var signature = IdentityService.Sign(identity.SigningPrivateKey, message);
            var isValid = IdentityService.Verify(identity.SigningPublicKey, message, signature);

            Assert.True(isValid);
        }

        [Fact]
        public void Verify_WithTamperedMessage_ReturnsFalse()
        {
            var identity = IdentityService.GenerateIdentity();
            var signature = IdentityService.Sign(identity.SigningPrivateKey, "original"u8.ToArray());

            var isValid = IdentityService.Verify(identity.SigningPublicKey, "tampered"u8.ToArray(), signature);

            Assert.False(isValid);
        }

        [Fact]
        public void Verify_WithAnotherIdentitysPublicKey_ReturnsFalse()
        {
            var signer = IdentityService.GenerateIdentity();
            var impostor = IdentityService.GenerateIdentity();
            var message = "who-am-i"u8.ToArray();
            var signature = IdentityService.Sign(signer.SigningPrivateKey, message);

            var isValid = IdentityService.Verify(impostor.SigningPublicKey, message, signature);

            Assert.False(isValid);
        }

        [Fact]
        public void Encapsulate_ThenDecapsulate_RecoversSameSharedSecret()
        {
            var recipient = IdentityService.GenerateIdentity();

            var (ciphertext, senderSecret) = IdentityService.Encapsulate(recipient.EncryptionPublicKey);
            var recipientSecret = IdentityService.Decapsulate(recipient.EncryptionPrivateKey, ciphertext);

            Assert.Equal(senderSecret, recipientSecret);
        }

        [Fact]
        public void Encapsulate_TwoCallsToSameRecipient_ProduceDifferentSharedSecrets()
        {
            var recipient = IdentityService.GenerateIdentity();

            var (_, firstSecret) = IdentityService.Encapsulate(recipient.EncryptionPublicKey);
            var (_, secondSecret) = IdentityService.Encapsulate(recipient.EncryptionPublicKey);

            Assert.NotEqual(firstSecret, secondSecret);
        }

        [Fact]
        public void EncryptMessage_ThenDecryptMessage_RecoversOriginalPlaintext()
        {
            var sender = IdentityService.GenerateIdentity();
            var recipient = IdentityService.GenerateIdentity();
            const string plaintext = "Selam! Bu mesaj ucdan uca sifreli.";

            var (encapsulatedKey, senderSecret) = IdentityService.Encapsulate(recipient.EncryptionPublicKey);
            var ciphertext = IdentityService.EncryptMessage(senderSecret, plaintext);

            var recipientSecret = IdentityService.Decapsulate(recipient.EncryptionPrivateKey, encapsulatedKey);
            var decrypted = IdentityService.DecryptMessage(recipientSecret, ciphertext);

            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void EncryptMessage_TwoCallsWithSameSecretAndPlaintext_ProduceDifferentCiphertext()
        {
            var recipient = IdentityService.GenerateIdentity();
            var (_, sharedSecret) = IdentityService.Encapsulate(recipient.EncryptionPublicKey);

            var first = IdentityService.EncryptMessage(sharedSecret, "same message");
            var second = IdentityService.EncryptMessage(sharedSecret, "same message");

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void DecryptMessage_WithWrongSharedSecret_Throws()
        {
            var recipient = IdentityService.GenerateIdentity();
            var (_, rightSecret) = IdentityService.Encapsulate(recipient.EncryptionPublicKey);
            var (_, wrongSecret) = IdentityService.Encapsulate(recipient.EncryptionPublicKey);
            var ciphertext = IdentityService.EncryptMessage(rightSecret, "top secret");

            Assert.ThrowsAny<Exception>(() => IdentityService.DecryptMessage(wrongSecret, ciphertext));
        }
    }
}
