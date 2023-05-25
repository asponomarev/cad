using System.Security.Cryptography;
using System.Text;

namespace UhlnocsServer.Utils
{
    public static class HashUtils
    {
        public static string GetHashCode(string value)
        {
            // this method creates hash of string value using sha256 algorithm and returns string with hexadecimal representation of hashed data
            var hash = string.Empty;
            using (var hashAlgorithm = SHA256.Create())
            {
                byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));

                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; ++i)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                hash = sBuilder.ToString();
            }
            return hash;
        }
    }
}
