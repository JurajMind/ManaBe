using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Device;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;

namespace smartHookah.Services.Device
{
    public class DevicePictureService : IDevicePictureService
    {
        private SmartHookahContext db;

        public DevicePictureService(SmartHookahContext db)
        {
            this.db = db;
        }


        public async Task<StandPicture> FindStandPicture(int id)
        {
            var picture = await this.db.Hookahs.Include(b => b.Setting.Picture).Where(a => a.Id == id)
                .Select(a => a.Setting.Picture).FirstOrDefaultAsync();
            return picture;
        }

        public async Task<StandPicture> GetStandPicture(int deviceId)
        {
            var device = await this.db.Hookahs.FindAsync(deviceId);

            if (device == null)
            {
                throw new ManaException(ErrorCodes.DeviceNotFound,$"Device with id {deviceId} was not found");
            }

            return device.Setting.Picture;
        }

        public async Task<bool> SetStandPicture(int deviceId, int pictureId)
        {
            var picture = await this.db.StandPictures.FindAsync(pictureId);
            var device = await this.db.Hookahs.FindAsync(deviceId);

            if (device == null)
            {
                throw new ManaException(ErrorCodes.DeviceNotFound, $"Device with id {deviceId} was not found");
            }
        
            if (picture == null)
            {
                return false;
            }

            device.Setting.PictureId = picture.Id;

            this.db.HookahSettings.AddOrUpdate(device.Setting);
            await this.db.SaveChangesAsync();
            return true;
        }

        public async Task<ICollection<StandPicture>> GetAllPictures(string type)
        {
            return await this.db.StandPictures.ToListAsync();
        }
    }

    public interface IDevicePictureService
    {
        Task<StandPicture> FindStandPicture(int id);

        Task<StandPicture> GetStandPicture(int deviceId);

        Task<ICollection<StandPicture>> GetAllPictures(string type);

        Task<bool> SetStandPicture(int deviceId, int pictureId);
    }
}