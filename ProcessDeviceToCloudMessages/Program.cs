using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace ProcessDeviceToCloudMessages
{
    class Program
    {
        static void Main(string[] args)
        {
            string iotHubConnectionString = ConfigurationManager.AppSettings["IoTConnectionString"];
            string iotHubD2cEndpoint = "messages/events";
            StoreEventProcessor.StorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            StoreEventProcessor.ServiceBusConnectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"];

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint, EventHubConsumerGroup.DefaultGroupName, iotHubConnectionString, StoreEventProcessor.StorageConnectionString, "messages-events");
            Console.WriteLine("Registering EventProcessor...");
            eventProcessorHost.RegisterEventProcessorAsync<StoreEventProcessor>().Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
