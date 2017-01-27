using System;
using System.Text;

namespace ShopAtHome.UnitTestHelper
{
    /// <summary>
    /// Provides access to various test data creation functions
    /// </summary>
    public class TestData
    {
        private static readonly DateTime _minDate = new DateTime(1995, 1, 1);

        /// <summary>
        /// Psuedo-random number generator
        /// </summary>
        public static readonly Random Prng = new Random();

        /// <summary>
        /// Returns a random integer between 1 and IntMax
        /// </summary>
        /// <returns></returns>
        public static int RandomPositiveNumber()
        {
            return RandomNumber(1);
        }

        /// <summary>
        /// Returns a random integer within the provided boundaries
        /// </summary>
        /// <returns></returns>
        public static int RandomNumber(int min = int.MinValue, int max = int.MaxValue)
        {
            return Prng.Next(min, max);
        }

        /// <summary>
        /// Returns a random date in the given range.
        /// </summary>
        /// <param name="minDate">The minimum date.</param>
        /// <param name="maxDate">The maximum date.</param>
        /// <returns></returns>
        public static DateTime RandomDate(DateTime minDate, DateTime maxDate)
        {
            int range = (maxDate - minDate).Days;
            return minDate.AddDays(Prng.Next(range));
        }

        /// <summary>
        /// Returns a random date between 1/1/1995 and today.
        /// </summary>
        /// <returns></returns>
        public static DateTime RandomPastDate()
        {
            return RandomDate(_minDate, DateTime.Today);
        }

        /// <summary>
        /// Returns a random date between today and the end of time
        /// </summary>
        /// <returns></returns>
        public static DateTime RandomFutureDate()
        {
            return RandomDate(DateTime.Today, DateTime.MaxValue);
        }

        /// <summary>
        /// Returns a random date between 1/1/1995 and the end of time
        /// </summary>
        /// <returns></returns>
        public static DateTime RandomDate()
        {
            return RandomDate(_minDate, DateTime.MaxValue);
        }

        /// <summary>
        /// Returns a random unicode string
        /// </summary>
        /// <returns></returns>
        public static string RandomString()
        {
            var bytes = new byte[8];
            Prng.NextBytes(bytes);
            return Encoding.Unicode.GetString(bytes);
        }

        /// <summary>
        /// Initiates the construction of a T by filling its properties with random data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TestDataBuilder<T> Build<T>() where T : class, new()
        {
            return new TestDataBuilder<T>();
        }
    }

    /// <summary>
    /// A partially constructed T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TestDataBuilder<T> where T : class, new()
    {
        private readonly T _result;

        /// <summary>
        /// Initializes the enclosed T with random data
        /// </summary>
        public TestDataBuilder()
        {
            _result = GenFu.GenFu.New<T>();
        }

        /// <summary>
        /// Executes the provided action on the underlying T
        /// </summary>
        /// <param name="doThing"></param>
        /// <returns></returns>
        public TestDataBuilder<T> With(Action<T> doThing)
        {
            doThing(_result);
            return this;
        }

        /// <summary>
        /// Returns the underlying T
        /// </summary>
        /// <returns></returns>
        public T Finish()
        {
            return _result;
        }
    }
}
