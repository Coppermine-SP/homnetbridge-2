using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace CloudInteractive.HomNetBridge.Util
{
    public static class Helper
    {
        public static byte[] HexToByte(string hex)
        {
            hex = hex.Trim()
                .Replace("-", String.Empty)
                .Replace(" ", String.Empty);

            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static bool CompareBoolArray(bool[] lhs, bool[] rhs)
        {
            if (lhs.Length != rhs.Length) return false;

            for (int i = 0; i < lhs.Length; i++) if (lhs[i] != rhs[i]) return false;
            return true;
        }
        public static string GetUrlParameter(string key, string value)
        {
            NameValueCollection parameters = HttpUtility.ParseQueryString(value);
            return parameters[key];
        }
    }
}
