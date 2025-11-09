namespace Aeter.Ratio
{
    public static class StringExtensions
    {
        public static bool OEqual(this string a, string b) => string.Equals(a, b, System.StringComparison.Ordinal);
        public static bool IEqual(this string a, string b) => string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
    }
}
