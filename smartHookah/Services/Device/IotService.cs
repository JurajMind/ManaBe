namespace smartHookah.Services.Device
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Azure.Devices;

    public class IotService : IIotService
    {
        private readonly RegistryManager registryManager;

        private readonly ServiceClient serviceClient;

        public IotService()
        {
            var iotConnectionString = ConfigurationManager.AppSettings["IoTConnectionString"];
            this.serviceClient = ServiceClient.CreateFromConnectionString(iotConnectionString);
            this.registryManager = RegistryManager.CreateFromConnectionString(iotConnectionString);
        }

        public async Task<IEnumerable<Device>> GetDevices(IEnumerable<string> deviceIds)
        {
            var devices = await this.registryManager.GetDevicesAsync(100);
            return devices.Where(d => deviceIds.Contains(d.Id));
        }

        public async Task<Device> GetDevidce(string deviceId)
        {
            return await this.registryManager.GetDeviceAsync(deviceId);
        }

        public async Task<bool> GetOnlineState(string deviceId)
        {
            var devices = await this.GetDevidce(deviceId);
            return devices?.ConnectionState == DeviceConnectionState.Connected;
        }

        public async Task<Dictionary<string, bool>> GetOnlineStates(IEnumerable<string> deviceIds)
        {
            var devices = await this.registryManager.GetDevicesAsync(100);
            return devices.Where(d => deviceIds.Contains(d.Id)).ToDictionary(
                a => a.Id,
                b => b.ConnectionState == DeviceConnectionState.Connected);
        }

        public async Task SendMsgToDevice(string deviceId, string message)
        {
            var serviceMessage =
                new Message(Encoding.ASCII.GetBytes(message))
                    {
                        Ack = DeliveryAcknowledgement.Full,
                        MessageId = Guid.NewGuid().ToString()
                    };
            await this.serviceClient.SendAsync(deviceId, serviceMessage);
            await this.serviceClient.CloseAsync();
        }
    }
}