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
        Task SetBrightness(string deviceId, int brightness, PufType state);
        Task SetSpeed(string deviceId, int speed, PufType state);
        Task SetColor(string deviceId, Color color, PufType state);
        Task Sleep(string deviceId);
        Task Restart(string deviceId);
        Task SetMode(string deviceId, int mode);
        Task ShowQrCode(string deviceId);
        List<Animation> GetAnimations();
        Animation GetAnimation(int id);
        int GetDeviceVersion(string id);

        Task<Dictionary<string, bool>> GetOnlineStates(IEnumerable<string> deviceIds);
    }
}