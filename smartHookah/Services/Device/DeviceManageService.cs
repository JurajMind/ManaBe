using smartHookah.Models.Db;
using smartHookah.Models.Dto.Device;
using smartHookah.Services.SmokeSession;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace smartHookah.Services.Device
{
    public class DeviceManageService : IDeviceManageService
    {
        private readonly IIotService IotService;
        private readonly ISmokeSessionBgService sessionBgService;
        private readonly SmartHookahContext db;

        public DeviceManageService(IIotService iotService, SmartHookahContext db, ISmokeSessionBgService sessionBgService)
        {
            IotService = iotService;
            this.db = db;
            this.sessionBgService = sessionBgService;
        }

        private string GenerateCode()
        {
            var date = DateTime.Now;
            var prefix = $"tear{date.Year.ToString()}{date.Month.ToString("00")}{date.Day.ToString("00")}";
            var deviceIds = this.db.Hookahs.Select(a => a.Code).ToList();
            var batchCount = deviceIds.Count(a => a.StartsWith(prefix));
            return prefix + (batchCount + 1).ToString("000");
        }


        public async Task<DeviceCreationDto> CreateDevice()
        {
            var code = this.GenerateCode();
            var deviceIot = await this.IotService.CreateDevice(code);
            var patternHookah = db.Hookahs.First(a => a.Code == "pattern");
            var deviceDb = new Hookah(patternHookah);
            deviceDb.Code = code;
            deviceDb.UpdateType = UpdateType.Init;
            deviceDb.Name = "Manapipes Tear";
            deviceDb.Created = DateTime.UtcNow;
            this.db.Hookahs.Add(deviceDb);
            this.db.SaveChanges();
            var tokenToDevice = new DeviceCreationDto()
            {
                Id = code,
                Key = deviceIot.Authentication.SymmetricKey.PrimaryKey,
                Led = 32
            };
            await this.sessionBgService.InitSmokeSession(code);
            return tokenToDevice;

        }
    }
}