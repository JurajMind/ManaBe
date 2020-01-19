using smartHookah.Models.Dto.Device;
using System.Threading.Tasks;

namespace smartHookah.Services.Device
{
    public interface IDeviceManageService
    {
        Task<DeviceCreationDto> CreateDevice(bool debug);
    }
}