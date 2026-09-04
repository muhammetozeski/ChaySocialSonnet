using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Kems;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;

namespace ChaySocialSonnet.MainProject.Services.Identity
{
    /// <summary>
    /// Generates and operates on <see cref="ChayIdentity"/> keypairs using post-quantum algorithms
    /// (ML-DSA for signing, ML-KEM for key encapsulation) implemented entirely in managed code, so the
    /// exact same code runs on MAUI (Windows/Android) and in the browser (Blazor WebAssembly) — .NET's
    /// own native-backed PQC types are not available under WebAssembly.
    /// </summary>
    public static class IdentityService
    {
        /// <summary> Bytes of the SHA-256 signing-key hash kept in a <see cref="ChayIdentity.PublicId"/> (16 bytes = 128 bits, plenty collision-resistant for an account identity). </summary>
        const int PublicIdHashLengthBytes = 16;

        static readonly MLDsaParameters SigningParameterSet = MLDsaParameters.ml_dsa_65;
        static readonly MLKemParameters EncryptionParameterSet = MLKemParameters.ml_kem_768;

        /// <summary> Generates a brand-new account identity: an ML-DSA signing keypair and an ML-KEM encryption keypair. Call once, then keep the private keys on-device only. </summary>
        public static ChayIdentity GenerateIdentity()
        {
            var random = new SecureRandom();

            var signingGenerator = new MLDsaKeyPairGenerator();
            signingGenerator.Init(new MLDsaKeyGenerationParameters(random, SigningParameterSet));
            AsymmetricCipherKeyPair signingKeyPair = signingGenerator.GenerateKeyPair();
            var signingPublicKey = (MLDsaPublicKeyParameters)signingKeyPair.Public;
            var signingPrivateKey = (MLDsaPrivateKeyParameters)signingKeyPair.Private;

            var encryptionGenerator = new MLKemKeyPairGenerator();
            encryptionGenerator.Init(new MLKemKeyGenerationParameters(random, EncryptionParameterSet));
            AsymmetricCipherKeyPair encryptionKeyPair = encryptionGenerator.GenerateKeyPair();
            var encryptionPublicKey = (MLKemPublicKeyParameters)encryptionKeyPair.Public;
            var encryptionPrivateKey = (MLKemPrivateKeyParameters)encryptionKeyPair.Private;

            byte[] signingPublicKeyBytes = signingPublicKey.GetEncoded();

            return new ChayIdentity(
                PublicId: DerivePublicId(signingPublicKeyBytes),
                SigningPublicKey: signingPublicKeyBytes,
                SigningPrivateKey: signingPrivateKey.GetEncoded(),
                EncryptionPublicKey: encryptionPublicKey.GetEncoded(),
                EncryptionPrivateKey: encryptionPrivateKey.GetEncoded());
        }

        /// <summary> Derives the short, shareable identity string (lowercase hex) from a signing public key. </summary>
        public static string DerivePublicId(byte[] signingPublicKey)
        {
            byte[] hash = SHA256.HashData(signingPublicKey);
            return Convert.ToHexString(hash, 0, PublicIdHashLengthBytes).ToLowerInvariant();
        }

        /// <summary> Signs <paramref name="message"/> with an ML-DSA private key. The server can verify this without ever seeing the private key. </summary>
        public static byte[] Sign(byte[] signingPrivateKey, byte[] message)
        {
            var privateKey = MLDsaPrivateKeyParameters.FromEncoding(SigningParameterSet, signingPrivateKey);
            var signer = new MLDsaSigner(SigningParameterSet, deterministic: false);
            signer.Init(forSigning: true, privateKey);
            signer.BlockUpdate(message, 0, message.Length);
            return signer.GenerateSignature();
        }

        /// <summary> Verifies a signature produced by <see cref="Sign"/> against the matching ML-DSA public key. </summary>
        public static bool Verify(byte[] signingPublicKey, byte[] message, byte[] signature)
        {
            var publicKey = MLDsaPublicKeyParameters.FromEncoding(SigningParameterSet, signingPublicKey);
            var signer = new MLDsaSigner(SigningParameterSet, deterministic: false);
            signer.Init(forSigning: false, publicKey);
            signer.BlockUpdate(message, 0, message.Length);
            return signer.VerifySignature(signature);
        }

        /// <summary> Encapsulates a fresh shared secret to <paramref name="recipientEncryptionPublicKey"/> (ML-KEM). Send the returned ciphertext to the recipient; keep the shared secret to encrypt the actual message (e.g. with AES-256-GCM). </summary>
        public static (byte[] Ciphertext, byte[] SharedSecret) Encapsulate(byte[] recipientEncryptionPublicKey)
        {
            var publicKey = MLKemPublicKeyParameters.FromEncoding(EncryptionParameterSet, recipientEncryptionPublicKey);
            var encapsulator = new MLKemEncapsulator(EncryptionParameterSet);
            encapsulator.Init(publicKey);

            byte[] ciphertext = new byte[encapsulator.EncapsulationLength];
            byte[] sharedSecret = new byte[encapsulator.SecretLength];
            encapsulator.Encapsulate(ciphertext, 0, ciphertext.Length, sharedSecret, 0, sharedSecret.Length);

            return (ciphertext, sharedSecret);
        }

        /// <summary> Recovers the shared secret from a ciphertext produced by <see cref="Encapsulate"/>, using the matching ML-KEM private key. </summary>
        public static byte[] Decapsulate(byte[] encryptionPrivateKey, byte[] ciphertext)
        {
            var privateKey = MLKemPrivateKeyParameters.FromEncoding(EncryptionParameterSet, encryptionPrivateKey);
            var decapsulator = new MLKemDecapsulator(EncryptionParameterSet);
            decapsulator.Init(privateKey);

            byte[] sharedSecret = new byte[decapsulator.SecretLength];
            decapsulator.Decapsulate(ciphertext, 0, ciphertext.Length, sharedSecret, 0, sharedSecret.Length);

            return sharedSecret;
        }

        /// <summary> Length in bytes of the random nonce <see cref="EncryptMessage"/> prefixes to its output. </summary>
        const int GcmNonceLengthBytes = 12;
        const int GcmMacLengthBits = 128;

        /// <summary>
        /// Encrypts <paramref name="plaintext"/> with AES-256-GCM under an ML-KEM shared secret (from
        /// <see cref="Encapsulate"/>/<see cref="Decapsulate"/>). Uses BouncyCastle's own AES-GCM rather than
        /// <see cref="System.Security.Cryptography.AesGcm"/> for the same reason the rest of this class avoids
        /// .NET's built-in PQC types: a fully managed implementation behaves identically on MAUI and under
        /// Blazor WebAssembly. Returns a random 12-byte nonce followed by the ciphertext+tag.
        /// </summary>
        public static byte[] EncryptMessage(byte[] sharedSecret, string plaintext)
        {
            byte[] key = SHA256.HashData(sharedSecret);
            byte[] nonce = RandomNumberGenerator.GetBytes(GcmNonceLengthBytes);

            var cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(forEncryption: true, new AeadParameters(new KeyParameter(key), GcmMacLengthBits, nonce));

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] output = new byte[cipher.GetOutputSize(plaintextBytes.Length)];
            int length = cipher.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, output, 0);
            length += cipher.DoFinal(output, length);

            byte[] result = new byte[nonce.Length + length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(output, 0, result, nonce.Length, length);
            return result;
        }

        /// <summary> Reverses <see cref="EncryptMessage"/> given the same shared secret. </summary>
        public static string DecryptMessage(byte[] sharedSecret, byte[] nonceAndCiphertext)
        {
            byte[] key = SHA256.HashData(sharedSecret);
            byte[] nonce = nonceAndCiphertext[..GcmNonceLengthBytes];
            byte[] ciphertext = nonceAndCiphertext[GcmNonceLengthBytes..];

            var cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(forEncryption: false, new AeadParameters(new KeyParameter(key), GcmMacLengthBits, nonce));

            byte[] output = new byte[cipher.GetOutputSize(ciphertext.Length)];
            int length = cipher.ProcessBytes(ciphertext, 0, ciphertext.Length, output, 0);
            length += cipher.DoFinal(output, length);

            return Encoding.UTF8.GetString(output, 0, length);
        }
    }
}
