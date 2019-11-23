using Microsoft.Azure.Devices.Common;
using smartHookah.Models.Dto.Device;
using smartHookahCommon.Extensions;

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

        public async Task<IEnumerable<Device>> GetDevices(List<string> deviceIds)
        {
           
            var devices = await this.registryManager.GetDevicesAsync(deviceIds.Count);
            return devices.Where(d => deviceIds.Contains(d.Id));
        }

        public async Task<Device> GetDevice(string deviceId)
        {
            return await this.registryManager.GetDeviceAsync(deviceId);
        }

        public async Task<bool> GetOnlineState(string deviceId)
        {
            var devices = await this.GetDevice(deviceId);
            return devices?.ConnectionState == DeviceConnectionState.Connected;
        }

        public async Task<Dictionary<string, bool>> GetOnlineStates(IList<string> deviceIds)
        {
            var result = new Dictionary<string, bool>();
            var query = registryManager.CreateQuery("SELECT * FROM devices", 100);
            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsTwinAsync();
                foreach (var twin in page)
                {
                    result.Add(twin.DeviceId,twin.ConnectionState == DeviceConnectionState.Connected);
                }
                if(result.Keys.Count(deviceIds.Contains) == deviceIds.Count())
                    break;;
            }

            return result;
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

        public async Task<Device> CreateDevice(string code)
        {
            var primaryKey = CryptoKeyGenerator.GenerateKey(32);
            var secondaryKey = CryptoKeyGenerator.GenerateKey(32);
            var device = new Device(code)
            {
                Authentication = new AuthenticationMechanism
                {
                    SymmetricKey = {PrimaryKey = primaryKey, SecondaryKey = secondaryKey}
                }
            };
            return await registryManager.AddDeviceAsync(device);
        }
    }
}