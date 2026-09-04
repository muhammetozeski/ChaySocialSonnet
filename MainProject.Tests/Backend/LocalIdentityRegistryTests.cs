using ChaySocialSonnet.MainProject.Backend;
using ChaySocialSonnet.MainProject.Services.Identity;
using ChaySocialSonnet.Web.Backend;

namespace ChaySocialSonnet.MainProject.Tests.Backend
{
    public class LocalIdentityRegistryTests
    {
        [Fact]
        public async Task ChallengeResponse_WithCorrectSignature_Succeeds()
        {
            var registry = new LocalIdentityRegistry();
            var identity = IdentityService.GenerateIdentity();
            await registry.RegisterAsync(identity.PublicId, identity.SigningPublicKey, identity.EncryptionPublicKey, "Test User");

            var challenge = await registry.IssueChallengeAsync(identity.PublicId);
            var signature = IdentityService.Sign(identity.SigningPrivateKey, Convert.FromBase64String(challenge));
            var isValid = await registry.VerifyChallengeAsync(identity.PublicId, challenge, signature);

            Assert.True(isValid);
        }

        [Fact]
        public async Task VerifyChallenge_ReplayingTheSameChallenge_FailsSecondTime()
        {
            var registry = new LocalIdentityRegistry();
            var identity = IdentityService.GenerateIdentity();
            await registry.RegisterAsync(identity.PublicId, identity.SigningPublicKey, identity.EncryptionPublicKey, "Test User");
            var challenge = await registry.IssueChallengeAsync(identity.PublicId);
            var signature = IdentityService.Sign(identity.SigningPrivateKey, Convert.FromBase64String(challenge));

            var firstAttempt = await registry.VerifyChallengeAsync(identity.PublicId, challenge, signature);
            var replayAttempt = await registry.VerifyChallengeAsync(identity.PublicId, challenge, signature);

            Assert.True(firstAttempt);
            Assert.False(replayAttempt);
        }

        [Fact]
        public async Task VerifyChallenge_WithSignatureFromAnotherIdentity_Fails()
        {
            var registry = new LocalIdentityRegistry();
            var identity = IdentityService.GenerateIdentity();
            var impostor = IdentityService.GenerateIdentity();
            await registry.RegisterAsync(identity.PublicId, identity.SigningPublicKey, identity.EncryptionPublicKey, "Test User");
            var challenge = await registry.IssueChallengeAsync(identity.PublicId);
            var impostorSignature = IdentityService.Sign(impostor.SigningPrivateKey, Convert.FromBase64String(challenge));

            var isValid = await registry.VerifyChallengeAsync(identity.PublicId, challenge, impostorSignature);

            Assert.False(isValid);
        }

        [Fact]
        public async Task VerifyChallenge_ForUnregisteredIdentity_Fails()
        {
            var registry = new LocalIdentityRegistry();

            var isValid = await registry.VerifyChallengeAsync("never-registered", "irrelevant", [1, 2, 3]);

            Assert.False(isValid);
        }

        [Fact]
        public async Task GetSigningPublicKey_AfterRegister_ReturnsSameKey()
        {
            var registry = new LocalIdentityRegistry();
            var identity = IdentityService.GenerateIdentity();

            await registry.RegisterAsync(identity.PublicId, identity.SigningPublicKey, identity.EncryptionPublicKey, "Test User");
            var storedKey = await registry.GetSigningPublicKeyAsync(identity.PublicId);

            Assert.Equal(identity.SigningPublicKey, storedKey);
        }

        [Fact]
        public async Task RegisterAsync_WithMatchingSigningKey_ReturnsRegistered()
        {
            var registry = new LocalIdentityRegistry();
            var identity = IdentityService.GenerateIdentity();

            var result = await registry.RegisterAsync(identity.PublicId, identity.SigningPublicKey, identity.EncryptionPublicKey, "Test User");

            Assert.Equal(RegisterIdentityResult.Registered, result);
        }

        [Fact]
        public async Task RegisterAsync_WherePublicIdDoesNotMatchSigningKey_ReturnsPublicIdMismatch()
        {
            var registry = new LocalIdentityRegistry();
            var identity = IdentityService.GenerateIdentity();
            var somebodyElse = IdentityService.GenerateIdentity();

            var result = await registry.RegisterAsync(somebodyElse.PublicId, identity.SigningPublicKey, identity.EncryptionPublicKey, "Test User");

            Assert.Equal(RegisterIdentityResult.PublicIdMismatch, result);
        }

        [Fact]
        public async Task RegisterAsync_CalledTwiceWithTheSameSigningKey_ReturnsRegisteredBothTimes()
        {
            var registry = new LocalIdentityRegistry();
            var identity = IdentityService.GenerateIdentity();

            var first = await registry.RegisterAsync(identity.PublicId, identity.SigningPublicKey, identity.EncryptionPublicKey, "Test User");
            var second = await registry.RegisterAsync(identity.PublicId, identity.SigningPublicKey, identity.EncryptionPublicKey, "Test User (renamed)");

            Assert.Equal(RegisterIdentityResult.Registered, first);
            Assert.Equal(RegisterIdentityResult.Registered, second);
        }
    }
}
