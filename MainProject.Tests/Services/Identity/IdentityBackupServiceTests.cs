using ChaySocialSonnet.MainProject.Services.Identity;

namespace ChaySocialSonnet.MainProject.Tests.Services.Identity
{
    public class IdentityBackupServiceTests
    {
        [Fact]
        public void EncryptBackup_ThenDecryptBackup_WithCorrectPassphrase_RecoversTheSameIdentity()
        {
            var identity = IdentityService.GenerateIdentity();

            byte[] blob = IdentityBackupService.EncryptBackup(identity, "correct horse battery staple");
            var recovered = IdentityBackupService.DecryptBackup(blob, "correct horse battery staple");

            Assert.Equal(identity.PublicId, recovered.PublicId);
            Assert.Equal(identity.SigningPublicKey, recovered.SigningPublicKey);
            Assert.Equal(identity.SigningPrivateKey, recovered.SigningPrivateKey);
            Assert.Equal(identity.EncryptionPublicKey, recovered.EncryptionPublicKey);
            Assert.Equal(identity.EncryptionPrivateKey, recovered.EncryptionPrivateKey);
        }

        [Fact]
        public void DecryptBackup_WithWrongPassphrase_Throws()
        {
            var identity = IdentityService.GenerateIdentity();
            byte[] blob = IdentityBackupService.EncryptBackup(identity, "correct horse battery staple");

            Assert.ThrowsAny<Exception>(() => IdentityBackupService.DecryptBackup(blob, "wrong passphrase"));
        }

        [Fact]
        public void EncryptBackup_TwoCallsWithSamePassphrase_ProduceDifferentBlobs()
        {
            var identity = IdentityService.GenerateIdentity();

            byte[] first = IdentityBackupService.EncryptBackup(identity, "same passphrase");
            byte[] second = IdentityBackupService.EncryptBackup(identity, "same passphrase");

            Assert.NotEqual(first, second);
        }
    }
}
