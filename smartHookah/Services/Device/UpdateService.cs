using smartHookah.Controllers;
using smartHookah.Helpers;
using smartHookah.Models.Db;
using smartHookah.Services.Redis;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace smartHookah.Services.Device
{
    public class UpdateService : IUpdateService
    {
        private readonly SmartHookahContext db;
        private readonly IRedisService redisService;

        public UpdateService(SmartHookahContext db, IRedisService redisService)
        {
            this.db = db;
            this.redisService = redisService;
        }


        public async Task<ICollection<Update>> GetUpdates()
        {
            return await this.db.Updates.Where(a => a.Type == UpdateType.Stable || a.Type == UpdateType.Beta).ToListAsync();
        }

        public async Task<(Update stable, Update beta)> GetUpdateInitInfo()
        {
            var stableUpdateTask = await LatestTypedUpdate(UpdateType.Stable);
            var betaUpdateTask = await LatestTypedUpdate(UpdateType.Beta);

            return (stableUpdateTask, betaUpdateTask);


        }

        private async Task<Update> LatestTypedUpdate(UpdateType type)
        {
            return await this.db.Updates.Where(a => a.Type == type).OrderBy(a => a.ReleseDate).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateDevice(int deviceId, int updateId, Models.Db.Person user, bool isAdmin)
        {
            try
            {
                if (!isAdmin)
                {
                    var canUpdate = user.Hookahs.Any(a => a.Id == deviceId);
                    if (!canUpdate)
                        return false;
                }

                var hookah = await db.Hookahs.FindAsync(deviceId);
                var update = await db.Updates.FindAsync(updateId);
                var updateToken = Support.Support.RandomString(5);

                var updatePath = update.Path;

                var updateRedis = new UpdateController.UpdateRedis()
                {
                    FilePath = updatePath,
                    HookahCode = hookah.Code
                };

                this.redisService.StoreUpdate(updateToken, updateRedis);

                var msg = $"update:{updateToken}";

                await IotDeviceHelper.SendMsgToDevice(hookah.Code, msg);
            }
            catch (Exception e)
            {
                throw new ManaException(ErrorCodes.UpdateError, "Update was not successful", e);
            }


            return true;
        }
    }
}