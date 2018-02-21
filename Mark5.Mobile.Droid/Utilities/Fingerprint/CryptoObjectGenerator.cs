using System;
using Android.Security.Keystore;
using Android.Support.V4.Hardware.Fingerprint;
using Java.Security;
using Javax.Crypto;

namespace Mark5.Mobile.Droid.Utilities.Fingerprint
{
    public class CryptoObjectHelper
    {
        //Unique key name.
        static readonly string KeyName = "Mark5KeyName";
        static readonly string KeystoreName = "AndroidKeyStore";

        static readonly string KeyAlgorithm = KeyProperties.KeyAlgorithmAes;
        static readonly string BLOCK_MODE = KeyProperties.BlockModeCbc;
        static readonly string ENCRYPTION_PADDING = KeyProperties.EncryptionPaddingPkcs7;
        static readonly string TRANSFORMATION = KeyAlgorithm + "/" +
                                                BLOCK_MODE + "/" +
                                                ENCRYPTION_PADDING;
        readonly KeyStore keyStore;

        public CryptoObjectHelper()
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
            Cipher cipher = Cipher.GetInstance(TRANSFORMATION);
            try
            {
                cipher.Init(CipherMode.EncryptMode | CipherMode.DecryptMode, key);
            }
            catch (KeyPermanentlyInvalidatedException e)
            {
                keyStore.DeleteEntry(KeyName);
                if (retry)
                {
                    CreateCipher(false);
                }
                else
                {
                    throw new Exception("Could not create the cipher for fingerprint authentication.", e);
                }
            }
            return cipher;
        }

        IKey GetKey()
        {
            IKey secretKey;
            if (!keyStore.IsKeyEntry(KeyName))
            {
                CreateKey();
            }

            secretKey = keyStore.GetKey(KeyName, null);
            return secretKey;
        }

        void CreateKey()
        {
            KeyGenerator keyGen = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, KeystoreName);
            KeyGenParameterSpec keyGenSpec =
                new KeyGenParameterSpec.Builder(KeyName, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                    .SetBlockModes(BLOCK_MODE)
                    .SetEncryptionPaddings(ENCRYPTION_PADDING)
                    .SetUserAuthenticationRequired(true)
                    .Build();
            keyGen.Init(keyGenSpec);
            keyGen.GenerateKey();
        }
    }
}
