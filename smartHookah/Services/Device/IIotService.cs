namespace smartHookah.Services.Device
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Azure.Devices;

    public interface IIotService
    {
        Task<IEnumerable<Device>> GetDevices(IEnumerable<string> deviceIds);

        Task<Device> GetDevice(string deviceId);

        Task<bool> GetOnlineState(string deviceId);

        Task<Dictionary<string, bool>> GetOnlineStates(IEnumerable<string> deviceIds);

        Task SendMsgToDevice(string deviceId, string message);
    }
}