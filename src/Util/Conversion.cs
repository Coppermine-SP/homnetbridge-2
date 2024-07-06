using System.Linq;

namespace CloudInteractive.HomNetBridge.Util
{
    public static class Conversion
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
    }
}
