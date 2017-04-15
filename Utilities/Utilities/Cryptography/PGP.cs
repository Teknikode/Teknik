using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities.Cryptography
{
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
