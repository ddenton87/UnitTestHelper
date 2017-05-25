using Moq;

namespace ShopAtHome.UnitTestHelper
{
    public class MoqMocker : IMockCreator
    {
        public T Create<T>() where T : class
        {
            return new Mock<T>().Object;
        }
    }
}
