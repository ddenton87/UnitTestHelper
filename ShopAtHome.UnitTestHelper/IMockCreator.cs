namespace ShopAtHome.UnitTestHelper
{
    /// <summary>
    /// A layer of abstraction between this code and the various mocking libraries, e.q Moq or JustMock
    /// </summary>
    public interface IMockCreator
    {
        /// <summary>
        /// Returns a mocked implementation of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Create<T>() where T : class;
    }
}
