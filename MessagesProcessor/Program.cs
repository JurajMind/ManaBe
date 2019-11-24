using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;

namespace MessagesProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(ConfigurationManager.AppSettings["ServiceBusConnectionString"]);

            //Receiving a message
            MessageReceiver testQueueReceiver = factory.CreateMessageReceiver("iot");
            while (true)
            {
                using (BrokeredMessage retrievedMessage = testQueueReceiver.Peek())
                {
                    try
                    {
                        if (retrievedMessage == null)
                            continue;
                        //Console.WriteLine("Message(s) Retrieved: " + retrievedMessage.GetBody<string>());
                        Console.WriteLine(retrievedMessage.Properties["device"]);
                        //retrievedMessage.Complete();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        retrievedMessage.Abandon();
                    }
                }
            }
        }
    }
}
