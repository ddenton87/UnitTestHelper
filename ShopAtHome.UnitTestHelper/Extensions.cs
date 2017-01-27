using System;
using System.Text;

namespace ShopAtHome.UnitTestHelper
{

    /// <summary>
    /// Extension methods useful for generating or manipulating test data
    /// </summary>
    public static class TestingExtensions
    {
        /// <summary>
        /// Takes a string and randomizes the case value of each character using a coin flip
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RandomCase(this string value)
        {
            return value.RandomCase(() => TestData.Prng.NextDouble() > .5);
        }

        /// <summary>
        /// Takes a string and randomizes the case value of each character according to the results of the provided randomization function
        /// </summary>
        /// <param name="value"></param>
        /// <param name="randomizeFunction"></param>
        /// <returns></returns>
        public static string RandomCase(this string value, Func<bool> randomizeFunction)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var sb = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                sb.Append(randomizeFunction() ? FlipCase(character) : character);
            }

            return sb.ToString();
        }

        private static char FlipCase(char c)
        {
            return char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c);
        }
    }
}
