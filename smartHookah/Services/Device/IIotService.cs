namespace smartHookah.Services.Device
{
    using Microsoft.Azure.Devices;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IIotService
    {
        Task<IEnumerable<Device>> GetDevices(List<string> deviceIds);

        Task<Device> GetDevice(string deviceId);

        Task<bool> GetOnlineState(string deviceId);

        Task<Dictionary<string, bool>> GetOnlineStates(IList<string> deviceIds);

        Task SendMsgToDevice(string deviceId, string message);

        Task<Device> CreateDevice(string code);

        Task DeleteDevice(string code);
    }
}