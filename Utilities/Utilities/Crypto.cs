using System.Text;
using SecurityDriven.Inferno.Hash;
using SecurityDriven.Inferno.Mac;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Bcpg.OpenPgp;
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