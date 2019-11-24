using Moq;
using smartHookah.Models.Db;

namespace smartHookahTests
{
    class SmartHookahContextMock
    {
        public SmartHookahContext GetMock()
        {
            var mockContext = new Mock<SmartHookahContext>();

            return mockContext.Object;
        }
    }
}
