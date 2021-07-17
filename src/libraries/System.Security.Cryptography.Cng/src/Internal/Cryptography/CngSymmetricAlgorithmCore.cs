// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using Internal.NativeCrypto;

namespace Internal.Cryptography
{
    //
    // Common infrastructure for crypto symmetric algorithms that use Cng.
    //
    internal struct CngSymmetricAlgorithmCore
    {
        /// <summary>
        /// Configures the core to use plaintext keys (to be auto-generated when first needed.)
        /// </summary>
        public CngSymmetricAlgorithmCore(ICngSymmetricAlgorithm outer)
        {
            _outer = outer;

            _keyName = null; // Setting _keyName to null signifies that this object is based on a plaintext key, not a stored CNG key.
            _provider = null;
            _optionOptions = CngKeyOpenOptions.None;
        }

        /// <summary>
        /// Constructs the core to use a stored CNG key.
        /// </summary>
        public CngSymmetricAlgorithmCore(ICngSymmetricAlgorithm outer, string keyName, CngProvider provider, CngKeyOpenOptions openOptions)
        {
            if (keyName == null)
                throw new ArgumentNullException(nameof(keyName));
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _outer = outer;

            _keyName = keyName;
            _provider = provider;
            _optionOptions = openOptions;

            using (CngKey cngKey = ProduceCngKey())
            {
                CngAlgorithm actualAlgorithm = cngKey.Algorithm;
                string algorithm = _outer.GetNCryptAlgorithmIdentifier();

                if (algorithm != actualAlgorithm.Algorithm)
                    throw new CryptographicException(SR.Format(SR.Cryptography_CngKeyWrongAlgorithm, actualAlgorithm.Algorithm, algorithm));

                _outer.BaseKeySize = cngKey.KeySize;
            }
        }

        /// <summary>
        /// Note! This can and likely will throw if the algorithm was given a hardware-based key.
        /// </summary>
        public byte[] GetKeyIfExportable()
        {
            if (KeyInPlainText)
            {
                return _outer.BaseKey;
            }
            else
            {
                using (CngKey cngKey = ProduceCngKey())
                {
                    return cngKey.GetSymmetricKeyDataIfExportable(_outer.GetNCryptAlgorithmIdentifier());
                }
            }
        }

        public void SetKey(byte[] key)
        {
            _outer.BaseKey = key;
            _keyName = null; // Setting _keyName to null signifies that this object is now based on a plaintext key, not a stored CNG key.
        }

        public void SetKeySize(int keySize, ICngSymmetricAlgorithm outer)
        {
            // Warning: This gets invoked once before "this" is initialized, due to Aes(), DES(), etc., setting the KeySize property in their
            // nullary constructor. That's why we require "outer" being passed as parameter.
            Debug.Assert(_outer == null || _outer == outer);

            outer.BaseKeySize = keySize;
            _keyName = null; // Setting _keyName to null signifies that this object is now based on a plaintext key, not a stored CNG key.
        }

        public void GenerateKey()
        {
            byte[] key = Helpers.GenerateRandom(_outer.BaseKeySize.BitSizeToByteSize());
            SetKey(key);
        }

        public void GenerateIV()
        {
            byte[] iv = Helpers.GenerateRandom(_outer.BlockSize.BitSizeToByteSize());
            _outer.IV = iv;
        }

        public ICryptoTransform CreateEncryptor()
        {
            return CreateCryptoTransform(encrypting: true);
        }

        public ICryptoTransform CreateDecryptor()
        {
            return CreateCryptoTransform(encrypting: false);
        }

        public ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[]? rgbIV)
        {
            return CreateCryptoTransform(rgbKey, rgbIV, encrypting: true, _outer.Padding, _outer.Mode, _outer.FeedbackSize);
        }

        public ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[]? rgbIV)
        {
            return CreateCryptoTransform(rgbKey, rgbIV, encrypting: false, _outer.Padding, _outer.Mode, _outer.FeedbackSize);
        }

        private ICryptoTransform CreateCryptoTransform(bool encrypting)
        {
            if (KeyInPlainText)
            {
                return CreateCryptoTransform(_outer.BaseKey, _outer.IV, encrypting, _outer.Padding, _outer.Mode, _outer.FeedbackSize);
            }

            return CreatePersistedCryptoTransformCore(ProduceCngKey, _outer.IV, encrypting, _outer.Padding, _outer.Mode, _outer.FeedbackSize);
        }

        public UniversalCryptoTransform CreateCryptoTransform(byte[]? iv, bool encrypting, PaddingMode padding, CipherMode mode, int feedbackSizeInBits)
        {
            if (KeyInPlainText)
            {
                return CreateCryptoTransform(_outer.BaseKey, iv, encrypting, padding, mode, feedbackSizeInBits);
            }

            return CreatePersistedCryptoTransformCore(ProduceCngKey, iv, encrypting, padding, mode, feedbackSizeInBits);
        }

        private UniversalCryptoTransform CreateCryptoTransform(byte[] rgbKey, byte[]? rgbIV, bool encrypting, PaddingMode padding, CipherMode mode, int feedbackSizeInBits)
        {
            if (rgbKey == null)
                throw new ArgumentNullException(nameof(rgbKey));

            ValidateFeedbackSize(mode, feedbackSizeInBits);

            byte[] key = rgbKey.CloneByteArray();

            long keySize = key.Length * (long)BitsPerByte;
            if (keySize > int.MaxValue || !((int)keySize).IsLegalSize(_outer.LegalKeySizes))
                throw new ArgumentException(SR.Cryptography_InvalidKeySize, nameof(rgbKey));

            if (_outer.IsWeakKey(key))
                throw new CryptographicException(SR.Cryptography_WeakKey);

            if (rgbIV != null && rgbIV.Length != _outer.BlockSize.BitSizeToByteSize())
                throw new ArgumentException(SR.Cryptography_InvalidIVSize, nameof(rgbIV));

            // CloneByteArray is null-preserving. So even when GetCipherIv returns null the iv variable
            // is correct, and detached from the input parameter.
            byte[]? iv = mode.GetCipherIv(rgbIV).CloneByteArray();

            key = _outer.PreprocessKey(key);

            return CreateEphemeralCryptoTransformCore(key, iv, encrypting, padding, mode, feedbackSizeInBits);
        }

        private UniversalCryptoTransform CreateEphemeralCryptoTransformCore(byte[] key, byte[]? iv, bool encrypting, PaddingMode padding, CipherMode mode, int feedbackSizeInBits)
        {
            int blockSizeInBytes = _outer.BlockSize.BitSizeToByteSize();
            SafeAlgorithmHandle algorithmModeHandle = _outer.GetEphemeralModeHandle(mode, feedbackSizeInBits);

            BasicSymmetricCipher cipher = new BasicSymmetricCipherBCrypt(
                algorithmModeHandle,
                mode,
                blockSizeInBytes,
                _outer.GetPaddingSize(mode, feedbackSizeInBits),
                key,
                ownsParentHandle: false,
                iv,
                encrypting);

            return UniversalCryptoTransform.Create(padding, cipher, encrypting);
        }

        private UniversalCryptoTransform CreatePersistedCryptoTransformCore(Func<CngKey> cngKeyFactory, byte[]? iv, bool encrypting, PaddingMode padding, CipherMode mode, int feedbackSizeInBits)
        {
            // note: iv is guaranteed to be cloned before this method, so no need to clone it again

            ValidateFeedbackSize(mode, feedbackSizeInBits);
            Debug.Assert(mode == CipherMode.CFB ? feedbackSizeInBits == 8 : true);

            int blockSizeInBytes = _outer.BlockSize.BitSizeToByteSize();
            BasicSymmetricCipher cipher = new BasicSymmetricCipherNCrypt(
                cngKeyFactory,
                mode,
                blockSizeInBytes,
                iv,
                encrypting,
                _outer.GetPaddingSize(mode, feedbackSizeInBits));
            return UniversalCryptoTransform.Create(padding, cipher, encrypting);
        }

        private CngKey ProduceCngKey()
        {
            Debug.Assert(!KeyInPlainText);

            return CngKey.Open(_keyName!, _provider!, _optionOptions);
        }

        private bool KeyInPlainText
        {
            get { return _keyName == null; }
        }

        private void ValidateFeedbackSize(CipherMode mode, int feedbackSizeInBits)
        {
            if (mode != CipherMode.CFB)
                return;

            if (KeyInPlainText)
            {
                if (!_outer.IsValidEphemeralFeedbackSize(feedbackSizeInBits))
                {
                    throw new CryptographicException(string.Format(SR.Cryptography_CipherModeFeedbackNotSupported, feedbackSizeInBits, CipherMode.CFB));
                }
            }
            else if (feedbackSizeInBits != 8)
            {
                // Persisted CNG keys in CFB mode always use CFB8 when in CFB mode,
                // so require the feedback size to be set to 8.
                throw new CryptographicException(string.Format(SR.Cryptography_CipherModeFeedbackNotSupported, feedbackSizeInBits, CipherMode.CFB));
            }
        }

        private readonly ICngSymmetricAlgorithm _outer;

        // If using a stored CNG key, these fields provide the CngKey.Open() parameters. If using a plaintext key, _keyName is set to null.
        private string? _keyName;
        private readonly CngProvider? _provider;
        private readonly CngKeyOpenOptions _optionOptions;

        private const int BitsPerByte = 8;
    }
}
