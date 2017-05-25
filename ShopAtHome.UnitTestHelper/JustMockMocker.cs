using Telerik.JustMock;

namespace ShopAtHome.UnitTestHelper
{
    public class JustMockMocker : IMockCreator
    {
        public T Create<T>() where T : class
        {
            return Mock.Create<T>();
        }
    }
}
