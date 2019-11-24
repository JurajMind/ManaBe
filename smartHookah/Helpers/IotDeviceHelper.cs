using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smartHookah.Helpers
{
    public class IotDeviceHelper
    {
        public static async Task SendMsgToDevice(string deviceId, string message)
        {
            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(ConfigurationManager.AppSettings["IoTConnectionString"]);
            var serviceMessage = new Message(Encoding.ASCII.GetBytes(message))
            {
                Ack = DeliveryAcknowledgement.Full,
                MessageId = Guid.NewGuid().ToString()
            };
            await serviceClient.SendAsync(deviceId, serviceMessage);
            await serviceClient.CloseAsync();
        }

        public static async Task<List<string>> GetState(List<string> deviceIds)
        {
            var registryManager = RegistryManager.CreateFromConnectionString(ConfigurationManager.AppSettings["IoTConnectionString"]);
            var devices = await registryManager.GetDevicesAsync(100);

            return devices.Where(d => deviceIds.Contains(d.Id) && d.ConnectionState == DeviceConnectionState.Connected)
                .Select(d => d.Id).ToList();

        }


    }
}