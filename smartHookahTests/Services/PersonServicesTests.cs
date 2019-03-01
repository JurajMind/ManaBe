using smartHookah.Models.Db;

namespace smartHookahTests.Services
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Security.Principal;

    using Microsoft.AspNet.Identity;
    using Microsoft.Owin;

    using Moq;

    using NUnit.Framework;

    using smartHookah;
    using smartHookah.Models;
    using smartHookah.Services.Device;
    using smartHookah.Services.Person;
    using smartHookah.Services.Redis;

    using smartHookahCommon;

    [TestFixture]
    internal class PersonServicesTests
    {
        [Test]
        [Ignore("Extensions")]
        public void GetPersonTest()
        {
            var userId = "1";

            var db = new Mock<SmartHookahContext>();

            var data = new List<Person> { new Person { Id = 1, Name = "Test" } }.AsQueryable();
            var mockSet = new Mock<DbSet<Person>>();
            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);

            var owinMock = new Mock<IOwinContext>(MockBehavior.Strict);
            var principal = new Mock<IPrincipal>(MockBehavior.Strict);
            var identityMock = new Mock<IIdentity>(MockBehavior.Strict);
            var redisMock = new Mock<IRedisService>(MockBehavior.Strict);
            identityMock.Setup(s => s.GetUserId()).Returns(userId);
            principal.SetupGet(a => a.Identity).Returns(identityMock.Object);


            var owinContextExtensionsWrapper = new Mock<IOwinContextExtensionsWrapper>(MockBehavior.Strict);
            owinContextExtensionsWrapper.Setup(s => s.FindById<ApplicationUser, string>(userId,owinMock.Object))
                .Returns(new ApplicationUser { Id = userId });

            db.Setup(d => d.Persons).Returns(mockSet.Object);

            var service = new PersonService(
                db.Object,
                owinMock.Object,
                principal.Object,
                deviceServiceMock.Object,
                redisMock.Object);

            var result = service.GetCurentPerson();

        }
    }
}