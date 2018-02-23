using System;
using Android.Security.Keystore;
using Android.Support.V4.Hardware.Fingerprint;
using Java.Security;
using Javax.Crypto;

namespace Mark5.Mobile.Droid.Utilities.Fingerprint
{
    public class CryptoObjectUtility
    {
        static readonly string KeyName = "Mark5KeyName";
        static readonly string KeystoreName = "AndroidKeyStore";

        static readonly string KeyAlgorithm = KeyProperties.KeyAlgorithmAes;
        static readonly string BlockMode = KeyProperties.BlockModeCbc;
        static readonly string Encryptionpadding = KeyProperties.EncryptionPaddingPkcs7;
        static readonly string Transformation = KeyAlgorithm + "/" +
                                                BlockMode + "/" +
                                                Encryptionpadding;
        readonly KeyStore keyStore;

        public CryptoObjectUtility()
        {
            keyStore = KeyStore.GetInstance(KeystoreName);
            keyStore.Load(null);
        }

        public FingerprintManagerCompat.CryptoObject BuildCryptoObject()
        {
            Cipher cipher = CreateCipher();
            return new FingerprintManagerCompat.CryptoObject(cipher);
        }

        Cipher CreateCipher(bool retry = true)
        {
            IKey key = GetKey();
            Cipher cipher = Cipher.GetInstance(Transformation);
            try
            {
                cipher.Init(CipherMode.EncryptMode | CipherMode.DecryptMode, key);
            }
            catch (KeyPermanentlyInvalidatedException e)
            {
                keyStore.DeleteEntry(KeyName);

                if (retry)
                    CreateCipher(false);
                else
                    throw new Exception("Could not create the cipher for fingerprint authentication.", e);
            }
            return cipher;
        }

        IKey GetKey()
        {
            IKey secretKey;
            if (!keyStore.IsKeyEntry(KeyName))
                CreateKey();
            
            secretKey = keyStore.GetKey(KeyName, null);
            return secretKey;
        }

        void CreateKey()
        {
            KeyGenerator keyGen = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, KeystoreName);
            KeyGenParameterSpec keyGenSpec =
                new KeyGenParameterSpec.Builder(KeyName, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                    .SetBlockModes(BlockMode)
                    .SetEncryptionPaddings(Encryptionpadding)
                    .SetUserAuthenticationRequired(true)
                    .Build();
            keyGen.Init(keyGenSpec);
            keyGen.GenerateKey();
        }
    }
}