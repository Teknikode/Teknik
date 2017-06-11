using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Utilities.Cryptography
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

        public static byte[] Hash(byte[] value)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            return md5.ComputeHash(value);
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
}
