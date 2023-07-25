using Microsoft.Azure.Devices.Common;

namespace smartHookah.Services.Device
{
    using Microsoft.Azure.Devices;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
            List<Device> result = new List<Device>();
            foreach (var id in deviceIds)
            {  
                result.Add(await this.GetDevice(id));
            }
            return result.Where(e => e != null).ToList() ;
        }

        public async Task<Device> GetDevice(string deviceId)
        {
            try
            {
                return await this.registryManager.GetDeviceAsync(deviceId);
            }
            catch (Exception)
            {
                return new Device(deviceId);
            }
          
        }

        public async Task<bool> GetOnlineState(string deviceId)
        {
            var devices = await this.GetDevice(deviceId);
            return devices?.ConnectionState == DeviceConnectionState.Connected;
        }

        public async Task<Dictionary<string, bool>> GetOnlineStates(IList<string> deviceIds)
        {
            var result = new Dictionary<string, bool>();
           
            try
            {
               foreach(var device in deviceIds) { 
                    result.Add(device,await GetOnlineState(device));
                }
            }
            catch (Exception e)
            {

            
            

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
                    SymmetricKey = { PrimaryKey = primaryKey, SecondaryKey = secondaryKey }
                }
            };
            return await registryManager.AddDeviceAsync(device);
        }

        public async Task DeleteDevice(string code)
        {
            var device = await this.GetDevice(code);
            if (device == null)
                return;
            await registryManager.RemoveDeviceAsync(device);
        }
    }
}