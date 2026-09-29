using System.Text;

namespace UrlShortener.Utilities
{
    public static class Base62Converter
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string Encode(long number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException(nameof(number), "Number must be non-negative.");

            if (number == 0)
                return "0";

            Span<char> buffer = stackalloc char[11];

            var position = buffer.Length;

            while (number > 0)
            {
                buffer[--position] = Alphabet[(int)(number % 62)];
                number /= 62;
            }

            return new string(buffer[position..]);
        }

        public static bool IsValid(string? input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length > 11)
                return false;

            foreach (var c in input)
            {
                if (!char.IsAsciiLetterOrDigit(c))
                    return false;
            }

            return true;
        }

        public static bool TryDecode(string? input, out long number)
        {
            number = 0;
            if (!IsValid(input))
                return false;

            foreach (var c in input!)
            {
                var index = Alphabet.IndexOf(c);
                if (index == -1)
                    return false;

                if (number > (long.MaxValue - index) / 62)
                    return false; // Prevent overflow

                number = number * 62 + index;
            }

            return true;
        }
    }
}
