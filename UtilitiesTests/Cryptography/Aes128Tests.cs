using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Utilities;
using Teknik.Utilities.Cryptography;
using Xunit;

namespace Teknik.Tests.UtilitiesTests.Cryptography
{
    public class Aes128Tests
    {
        [Fact]
        public void Aes128DataTest()
        {
            string secret = "426KOBTS66KYLFLQ";
            string key = "u1GIRvmnIFFHLov";

            // Get the Encryption Key from the git secret key
            byte[] keyBytes = MD5.Hash(Encoding.UTF8.GetBytes(key));

            // Modify the input secret
            byte[] secBytes = Encoding.UTF8.GetBytes(secret);

            // Generate the encrypted secret using AES CGM
            byte[] encValue = Aes128CFB.Encrypt(secBytes, keyBytes);
            string finalSecret = Convert.ToBase64String(encValue);

            // Decode it
            byte[] decodedSecret = Convert.FromBase64String(finalSecret);
            byte[] val = Aes128CFB.Decrypt(decodedSecret, keyBytes);
            string verify = Encoding.UTF8.GetString(val);

            Assert.Equal(secret, verify);
        }
    }
}
