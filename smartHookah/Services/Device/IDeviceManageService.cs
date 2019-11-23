using System.Threading.Tasks;
using smartHookah.Models.Dto.Device;

namespace smartHookah.Services.Device
{
    public interface IDeviceManageService
    {
        Task<DeviceCreationDto> CreateDevice();
    }
}