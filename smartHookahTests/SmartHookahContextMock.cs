using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using smartHookah.Models.Db;

namespace smartHookahTests
{
    using smartHookah.Models;

    class SmartHookahContextMock
    {
        public SmartHookahContext GetMock()
        {
            var mockContext = new Mock<SmartHookahContext>();

            return mockContext.Object;
        }
    }
}
