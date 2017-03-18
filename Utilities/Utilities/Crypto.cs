using System.Text;
using SecurityDriven.Inferno.Hash;
using SecurityDriven.Inferno.Mac;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;

namespace Teknik.Utilities
{
    public class MD5
    {
        public static string Hash(string value)
        {
            byte[] valBytes = Encoding.ASCII.GetBytes(value);
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] hashBytes = md5.ComputeHash(valBytes);

            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sBuilder.Append(hashBytes[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();

        }

        public static string FileHash(string filename)
        {
            try
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string DataHash(string data)
        {
            try
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    // convert string to stream
                    byte[] byteArray = Encoding.UTF8.GetBytes(data);
                    using (MemoryStream stream = new MemoryStream(byteArray))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    public class SHA384
    {
        public static byte[] Hash(string key, string value)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] data = Encoding.UTF8.GetBytes(value);

            byte[] result = new HMAC2(HashFactories.SHA384, keyBytes).ComputeHash(data);
            return result;
        }
    }

    public class SHA256
    {
        public static string Hash(string value)
        {
            byte[] valueBytes = Encoding.Unicode.GetBytes(value);
            return Hash(valueBytes);
        }

        public static string Hash(byte[] value)
        {
            HashAlgorithm hash = new SHA256CryptoServiceProvider();
            byte[] hashBytes = hash.ComputeHash(value);

            return Convert.ToBase64String(hashBytes);
        }

        public static byte[] Hash(Stream value)
        {
            HashAlgorithm hash = new SHA256CryptoServiceProvider();
            return hash.ComputeHash(value);
        }

        public static string Hash(string value, string salt1, string salt2)
        {
            SHA256Managed hash = new SHA256Managed();
            SHA1 sha1 = new SHA1Managed();
            // gen salt2 hash
            byte[] dataSalt2 = Encoding.UTF8.GetBytes(salt2);
            byte[] salt2Bytes = hash.ComputeHash(dataSalt2);
            string salt2Str = string.Empty;
            foreach (byte x in salt2Bytes)
            {
                salt2Str += String.Format("{0:x2}", x);
            }
            string dataStr = salt1 + value + salt2Str;
            byte[] dataStrBytes = Encoding.UTF8.GetBytes(dataStr);
            byte[] shaBytes = sha1.ComputeHash(dataStrBytes);
            string sha1Str = string.Empty;
            foreach (byte x in shaBytes)
            {
                sha1Str += String.Format("{0:x2}", x);
            }
            byte[] sha1Bytes = Encoding.UTF8.GetBytes(sha1Str);
            byte[] valueBytes = hash.ComputeHash(sha1Bytes);
            string hashString = string.Empty;
            foreach (byte x in valueBytes)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        public static System.Security.Cryptography.SHA256 CreateHashAlgorithm()
        {
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                return new SHA256CryptoServiceProvider();
            }

            return new SHA256Managed();
        }
    }

    public class AES
    {
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            return Decrypt(data, key, iv, "CTR", "NoPadding");
        }
        public static byte[] Decrypt(byte[] data, string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Decrypt(data, keyBytes, ivBytes, "CTR", "NoPadding");
        }
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv, string mode, string padding)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return ProcessCipher(false, stream, 1024, key, iv, mode, padding);
            }
        }


        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            return Encrypt(data, key, iv, "CTR", "NoPadding");
        }
        public static byte[] Encrypt(byte[] data, string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return Encrypt(data, keyBytes, ivBytes, "CTR", "NoPadding");
        }
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv, string mode, string padding)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return ProcessCipher(true, stream, 1024, key, iv, mode, padding);
            }
        }

        public static byte[] ProcessCipher(bool encrypt, Stream input, int chunkSize, byte[] key, byte[] iv, string mode, string padding)
        {
            // Create the cipher we are going to use
            IBufferedCipher cipher = CreateCipher(encrypt, key, iv, mode, padding);

            // Make sure the input stream is at the beginning
            input.Seek(0, SeekOrigin.Begin);

            // Initialize variables
            byte[] output = new byte[input.Length];
            int cipherOffset = 0;
            int bytesRead = 0;

            // Process the stream and save the bytes to the output
            do
            {
                int processedBytes = ProcessCipherBlock(cipher, input, chunkSize, output, cipherOffset, out bytesRead);
                cipherOffset += processedBytes;
            }
            while (bytesRead > 0);

            // Finalize processing of the cipher
            FinalizeCipherBlock(cipher, output, cipherOffset);

            return output;
        }

        public static void EncryptToFile(string filePath, Stream input, int chunkSize, byte[] key, byte[] iv, string mode, string padding)
        {
            IBufferedCipher cipher = CreateCipher(true, key, iv, mode, padding);

            // Make sure the input stream is at the beginning
            input.Seek(0, SeekOrigin.Begin);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                int processedBytes = 0;
                byte[] buffer = new byte[chunkSize];
                int bytesRead = 0;
                do
                {
                    processedBytes = ProcessCipherBlock(cipher, input, chunkSize, buffer, 0, out bytesRead);
                    if (processedBytes > 0)
                    {
                        // We have bytes, lets write them to the file
                        fileStream.Write(buffer, 0, processedBytes);

                        // Clear the buffer
                        Array.Clear(buffer, 0, chunkSize);
                    }
                }
                while (processedBytes > 0);

                // Clear the buffer
                Array.Clear(buffer, 0, chunkSize);
                
                // Finalize processing of the cipher
                processedBytes = FinalizeCipherBlock(cipher, buffer, 0);
                if (processedBytes > 0)
                {
                    // We have bytes, lets write them to the file
                    fileStream.Write(buffer, 0, processedBytes);
                }
            }
        }

        public static IBufferedCipher CreateCipher(bool encrypt, byte[] key, byte[] iv, string mode, string padding)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/" + mode + "/" + padding);

            cipher.Init(encrypt, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

            return cipher;
        }

        public static int ProcessCipherBlock(IBufferedCipher cipher, Stream input, int chunkSize, byte[] output, int outputOffset, out int bytesRead)
        {
            // Initialize buffer
            byte[] buffer = new byte[chunkSize];

            // Read the next block of data
            bytesRead = input.Read(buffer, 0, chunkSize);
            if (bytesRead > 0)
            {
                // process the cipher for the read block and add it to the output
                return cipher.ProcessBytes(buffer, 0, bytesRead, output, outputOffset);
            }

            return 0;
        }

        public static int FinalizeCipherBlock(IBufferedCipher cipher, byte[] output, int outputOffset)
        {
            // perform final action on cipher
            return cipher.DoFinal(output, outputOffset);
        }

        public static byte[] CreateKey(string password, string iv, int keySize = 256)
        {
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            return CreateKey(password, ivBytes, keySize);
        }
        public static byte[] CreateKey(string password, byte[] iv, int keySize = 256)
        {
            const int Iterations = 300;
            var keyGenerator = new Rfc2898DeriveBytes(password, iv, Iterations);
            return keyGenerator.GetBytes(keySize / 8);
        }
    }

    public static class PGP
    {
        public static bool IsPublicKey(string key)
        {
            bool isValid = false;

            try
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(key);
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    using (Stream decoderStream = PgpUtilities.GetDecoderStream(stream))
                    {
                        PgpPublicKeyRingBundle publicKeyBundle = new PgpPublicKeyRingBundle(decoderStream);
                        PgpPublicKey foundKey = GetFirstPublicKey(publicKeyBundle);

                        if (foundKey != null)
                        {
                            isValid = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isValid = false;
            }
            return isValid;
        }

        public static string GetFingerprint(string key)
        {
            string hexString = string.Empty;
            byte[] byteArray = Encoding.ASCII.GetBytes(key);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                using (Stream decoderStream = PgpUtilities.GetDecoderStream(stream))
                {
                    PgpPublicKeyRingBundle publicKeyBundle = new PgpPublicKeyRingBundle(decoderStream);
                    PgpPublicKey foundKey = GetFirstPublicKey(publicKeyBundle);

                    if (foundKey != null)
                    {
                        byte[] fing = foundKey.GetFingerprint();
                        hexString = Hex.ToHexString(fing);
                    }
                }
            }
            return hexString;
        }
        public static string GetFingerprint64(string key)
        {
            string fingerprint = GetFingerprint(key);
            if (fingerprint.Length > 16)
                fingerprint = fingerprint.Substring(fingerprint.Length - 16);
            return fingerprint;
        }

        private static PgpPublicKey GetFirstPublicKey(PgpPublicKeyRingBundle publicKeyRingBundle)
        {
            foreach (PgpPublicKeyRing kRing in publicKeyRingBundle.GetKeyRings())
            {
                var keys = kRing.GetPublicKeys();
                foreach (var key in keys)
                {
                    PgpPublicKey foundKey = (PgpPublicKey)key;
                    //PgpPublicKey key = kRing.GetPublicKeys()
                    //.Cast<PgpPublicKey>()
                    // .Where(k => k.IsEncryptionKey)
                    //  .FirstOrDefault();
                    if (foundKey != null && foundKey.IsEncryptionKey)
                        return foundKey;
                }
            }
            return null;
        }
    }
}