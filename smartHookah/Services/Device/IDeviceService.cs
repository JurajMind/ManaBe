using System.Collections.Generic;

namespace smartHookah.Services.Device
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using smartHookah.Helpers;
    using smartHookah.Models;

    public interface IDeviceService
    {
        Task SetAnimation(string deviceId, Animation animation, PufType state);
        List<Animation> GetAnimations();
        int GetDeviceVersion(string id);

        Task<Dictionary<string, bool>> GetOnlineStates(IEnumerable<string> deviceIds);
    }
}