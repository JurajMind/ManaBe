namespace smartHookah.Services.Device
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using smartHookah.Helpers;
    using smartHookah.Models;

    public interface IDeviceService
    {
        Task SetAnimation(string deviceId, Animation animation, PufType state);

        Task<Dictionary<string, bool>> GetOnlineStates(IEnumerable<string> deviceIds);
    }
}