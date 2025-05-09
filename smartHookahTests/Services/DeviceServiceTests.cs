﻿using smartHookah.Models.Db;
using smartHookah.Services.Messages;
using smartHookah.Services.Person;
using System.Collections.Generic;

namespace smartHookahTests.Services
{
    using Moq;
    using NUnit.Framework;
    using smartHookah.Helpers;
    using smartHookah.Services.Device;
    using smartHookah.Services.Redis;
    using smartHookahTests.Common;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [TestFixture]
    internal class DeviceServiceTests
    {
        [Test]
        public async Task SetAnimation()
        {
            // Paramets 
            var deviceId = "deviceId";
            PufType state = PufType.Idle;
            var animation = new Animation()
            {
                Id = 1,
                DisplayName = "Test Animation",
                Usage = AnimationUsage.All,
                VersionFrom = 1,
                VersionTo = int.MaxValue,
            };

            // db
            var hookahSetting = new DeviceSetting() { Id = 1, IdleAnimation = 0 };
            var db = new Mock<SmartHookahContext>();
            var hookahDataSet =
                new FakeDbSet<Hookah> { new Hookah() { Code = deviceId, Version = 100, Setting = hookahSetting } };

            var settingDataSet = new FakeDbSet<DeviceSetting> { hookahSetting };

            // Setup acces to dbSets
            db.Setup(dbContext => dbContext.Hookahs).Returns(hookahDataSet);
            db.Setup(dbContext => dbContext.HookahSettings).Returns(settingDataSet);

            // Mock for iot service
            var iotMock = new Mock<IIotService>(MockBehavior.Strict);

            // On iotService, SendToMsg shoud be called with given params, returning task
            iotMock.Setup(a => a.SendMsgToDevice(deviceId, $"led:{(int)state},{1},")).Returns(Task.FromResult(false));
            var redisMock = new Mock<IRedisService>(MockBehavior.Strict);
            var notificationMock = new Mock<ISignalNotificationService>(MockBehavior.Strict);
            notificationMock.Setup(s => s.SessionSettingsChanged(deviceId, It.IsAny<DeviceSetting>()));
            var personMock = new Mock<IPersonService>(MockBehavior.Loose);

            // Initialize new service with mock services
            var service = new DeviceService(db.Object, iotMock.Object, redisMock.Object, notificationMock.Object);

            // Executese
            await service.SetAnimation(deviceId, animation, state);

            // get "stored" setting value
            var storedItem = db.Object.HookahSettings.FirstOrDefault(a => a.Id == 1);

            // Check if is not null , and new value is stored
            Assert.That(storedItem, Is.Not.Null);
            Assert.That(storedItem.IdleAnimation, Is.EqualTo(1));

            // Check if we run all functions with corect params
            db.VerifyAll();
            iotMock.VerifyAll();
        }

        [Test]
        public async Task SetAnimationWrongVersion()
        {
            // Paramets 
            var deviceId = "deviceId";
            PufType state = PufType.Idle;
            var animation = new Animation()
            {
                Id = 1,
                DisplayName = "Test Animation",
                Usage = AnimationUsage.All,
                VersionFrom = 1,
                VersionTo = 2,
            };

            // db
            var hookahSetting = new DeviceSetting() { Id = 1, IdleAnimation = 0 };
            var db = new Mock<SmartHookahContext>();
            var hookahDataSet =
                new FakeDbSet<Hookah> { new Hookah() { Code = deviceId, Version = 100, Setting = hookahSetting } };
            var settingDataSet = new FakeDbSet<DeviceSetting> { hookahSetting };

            // Setup acces to dbSets
            db.Setup(dbContext => dbContext.Hookahs).Returns(hookahDataSet);
            db.Setup(dbContext => dbContext.HookahSettings).Returns(settingDataSet);

            // Mock for iot service
            var iotMock = new Mock<IIotService>(MockBehavior.Strict);
            var redisMock = new Mock<IRedisService>(MockBehavior.Strict);
            // On iotService, SendToMsg shoud be called with given params, returning task
            var notificationMock = new Mock<ISignalNotificationService>(MockBehavior.Strict);
            var personMock = new Mock<IPersonService>(MockBehavior.Loose);

            // Initialize new service with mock services
            var service = new DeviceService(db.Object, iotMock.Object, redisMock.Object, notificationMock.Object);

            // Execute
            var ex = Assert.ThrowsAsync<NotSupportedException>(() => service.SetAnimation(deviceId, animation, state));
            Assert.That(
                ex.Message,
                Is.EqualTo($"Animation {animation.DisplayName} not supported by your Hookah OS version."));

            // get "stored" setting value
            var storedItem = db.Object.HookahSettings.FirstOrDefault(a => a.Id == 1);

            // Check if is not null , and new value is stored
            Assert.That(storedItem, Is.Not.Null);
            Assert.That(storedItem.IdleAnimation, Is.EqualTo(0));

            // Check if we run all functions with corect params
            db.VerifyAll();
            iotMock.VerifyAll();
        }

        [Test]
        public async Task SetAnimationWrongDeviece()
        {
            // Paramets 
            var deviceId = "deviceId";
            PufType state = PufType.Idle;
            var animation = new Animation()
            {
                Id = 1,
                DisplayName = "Test Animation",
                Usage = AnimationUsage.All,
                VersionFrom = 1,
                VersionTo = 1000,
            };

            // db
            var hookahSetting = new DeviceSetting() { Id = 1, IdleAnimation = 0 };
            var db = new Mock<SmartHookahContext>();
            var hookahDataSet = new FakeDbSet<Hookah>
                                    {
                                        new Hookah()
                                            {
                                                Code = deviceId + "error",
                                                Version = 100,
                                                Setting = hookahSetting
                                            }
                                    };
            var settingDataSet = new FakeDbSet<DeviceSetting> { hookahSetting };

            // Setup acces to dbSets
            db.Setup(dbContext => dbContext.Hookahs).Returns(hookahDataSet);
            db.Setup(dbContext => dbContext.HookahSettings).Returns(settingDataSet);

            // Mock for iot service
            var iotMock = new Mock<IIotService>(MockBehavior.Strict);

            // On iotService, SendToMsg shoud be called with given params, returning task
            var redisMock = new Mock<IRedisService>(MockBehavior.Strict);
            var notificationMock = new Mock<ISignalNotificationService>(MockBehavior.Strict);
            var personMock = new Mock<IPersonService>(MockBehavior.Loose);
            // Initialize new service with mock services
            var service = new DeviceService(db.Object, iotMock.Object, redisMock.Object, notificationMock.Object);

            // Execute
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => service.SetAnimation(deviceId, animation, state));

            // get "stored" setting value
            var storedItem = db.Object.HookahSettings.FirstOrDefault(a => a.Id == 1);

            // Check if is not null , and new value is stored
            Assert.That(storedItem, Is.Not.Null);
            Assert.That(storedItem.IdleAnimation, Is.EqualTo(0));

            // Check if we run all functions with corect params
            db.VerifyAll();
            iotMock.VerifyAll();
        }
    }
}