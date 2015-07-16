using System.Text;
using SecurityDriven.Inferno.Hash;
using SecurityDriven.Inferno.Mac;

namespace Teknik.Helpers
{
    public class SHA384
    {
        public static string Hash(string key, string value)
        {
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            byte[] data = Encoding.ASCII.GetBytes(value);

            byte[] result = new HMAC2(HashFactories.SHA384, keyBytes).ComputeHash(data);

            return Encoding.ASCII.GetString(result);
        }
    }
}