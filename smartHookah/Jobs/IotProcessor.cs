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
    static class IotProcessor
    {
        private static bool running = false;
        private static EventProcessorHost eventProcessorHost;
        public static void Start()
        {
            if(running)
                return;

            running = true;
            string iotHubConnectionString = ConfigurationManager.AppSettings["IoTConnectionString"];
            string iotHubD2cEndpoint = "messages/events";
            StoreEventProcessor.StorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            StoreEventProcessor.ServiceBusConnectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"];

            string eventProcessorHostName = Guid.NewGuid().ToString();
            eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint, EventHubConsumerGroup.DefaultGroupName, iotHubConnectionString, StoreEventProcessor.StorageConnectionString, "messages-events");
            Console.WriteLine("Registering EventProcessor...");
            eventProcessorHost.RegisterEventProcessorAsync<StoreEventProcessor>().Wait();
        }

        public static void End()
        {
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
            running = false;
        }
    }
}
