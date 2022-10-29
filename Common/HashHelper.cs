using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public static class HashHelper
    {
        public static string GetHash(string input)
        {
            using(var sha = SHA256.Create())
            {
                var data = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();

                foreach (var item in data)
                    sb.Append(item.ToString());

                return sb.ToString();
            }
        }

        public static bool Verify(string input,string hash)
        {
            var hashOfInput = GetHash(input);
            var comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hashOfInput, hash) == 0;
        }
    }
}
