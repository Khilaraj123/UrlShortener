using System.Text;

namespace UrlShortener.Utilities
{
    public static class Base62Converter
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly int Base = Alphabet.Length;

        public static string Encode(long number)
        {
            if (number <= 0)
                return Alphabet[0].ToString();

            var result = new StringBuilder();
            while(number > 0)
            {
                result.Insert(0, Alphabet[(int)(number % Base)]);
                number /= Base;
            }
            return result.ToString();
        }
    }
}
