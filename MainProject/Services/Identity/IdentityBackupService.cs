using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ChaySocialSonnet.MainProject.Services.Identity
{
    /// <summary>
    /// Encrypts a <see cref="ChayIdentity"/> into a self-contained backup file the user keeps themselves
    /// (their own drive/USB), so losing this device's on-device key store doesn't mean losing the account
    /// forever. The server never sees this blob or the passphrase protecting it — this is purely a
    /// device-to-device/offline recovery path.
    /// </summary>
    public static class IdentityBackupService
    {
        const int SaltLengthBytes = 16;
        const int Pbkdf2Iterations = 200_000;

        /// <summary> Layout: salt(16) + nonce(12) + ciphertext+tag, where nonce+ciphertext+tag is exactly <see cref="IdentityService.EncryptMessage"/>'s own output under the passphrase-derived key. </summary>
        public static byte[] EncryptBackup(ChayIdentity identity, string passphrase)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltLengthBytes);
            byte[] key = DeriveKey(passphrase, salt);
            string json = JsonSerializer.Serialize(identity);
            byte[] nonceAndCiphertext = IdentityService.EncryptMessage(key, json);

            byte[] blob = new byte[salt.Length + nonceAndCiphertext.Length];
            Buffer.BlockCopy(salt, 0, blob, 0, salt.Length);
            Buffer.BlockCopy(nonceAndCiphertext, 0, blob, salt.Length, nonceAndCiphertext.Length);
            return blob;
        }

        /// <summary> Reverses <see cref="EncryptBackup"/>. Throws if the passphrase is wrong or the blob is corrupt/tampered (the AES-GCM tag check fails first, well before JSON parsing would). </summary>
        public static ChayIdentity DecryptBackup(byte[] blob, string passphrase)
        {
            byte[] salt = blob[..SaltLengthBytes];
            byte[] nonceAndCiphertext = blob[SaltLengthBytes..];
            byte[] key = DeriveKey(passphrase, salt);

            string json = IdentityService.DecryptMessage(key, nonceAndCiphertext);
            return JsonSerializer.Deserialize<ChayIdentity>(json)
                ?? throw new InvalidOperationException("Backup decrypted but did not contain a valid identity.");
        }

        /// <summary>
        /// PBKDF2-HMAC-SHA256 via BouncyCastle rather than <see cref="Rfc2898DeriveBytes"/> — the same reason
        /// the rest of this app's crypto avoids .NET's native-backed APIs: <c>Rfc2898DeriveBytes.Pbkdf2</c>
        /// hangs indefinitely under Blazor WebAssembly instead of throwing or returning (confirmed live: the
        /// call never completes and never throws), so it's unusable here even though it works fine on MAUI.
        /// </summary>
        static byte[] DeriveKey(string passphrase, byte[] salt)
        {
            var generator = new Pkcs5S2ParametersGenerator(new Sha256Digest());
            generator.Init(Encoding.UTF8.GetBytes(passphrase), salt, Pbkdf2Iterations);
            var key = (KeyParameter)generator.GenerateDerivedMacParameters(256);
            return key.GetKey();
        }
    }
}
