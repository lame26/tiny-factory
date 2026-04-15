using System.Globalization;

namespace TinyFactory.Economy
{
    public static class MoneyFormatter
    {
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T" };

        public static string Format(int amount)
        {
            if (amount < 0)
            {
                return "-" + Format(-amount);
            }

            double value = amount;
            int suffixIndex = 0;

            while (value >= 1000d && suffixIndex < Suffixes.Length - 1)
            {
                value /= 1000d;
                suffixIndex++;
            }

            string numberFormat = value >= 100d || suffixIndex == 0 ? "0" : "0.#";
            return value.ToString(numberFormat, CultureInfo.InvariantCulture) + Suffixes[suffixIndex];
        }
    }
}
