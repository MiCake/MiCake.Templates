using System.Security.Cryptography;
using System.Text;

/*
 * Tips: This project is used to hold some utility classes that will be used later, like the HashHelper below.
 *
 */

namespace MiCakeTemplate.Util.Cryptography
{
    public static class HashHelper
    {
        public static string GetHash(string encryStr)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(encryStr);

            return Convert.ToBase64String(sha256.ComputeHash(bytes));
        }
    }
}
