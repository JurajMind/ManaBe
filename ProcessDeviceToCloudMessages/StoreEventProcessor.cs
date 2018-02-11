using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using smartHookah.Models;
using smartHookahCommon;

namespace ProcessDeviceToCloudMessages
{
    class StoreEventProcessor : IEventProcessor
    {
        private const int MAX_BLOCK_SIZE = 4 * 1024 * 1024;
        public static string StorageConnectionString;
        public static string ServiceBusConnectionString;

        private CloudBlobClient blobClient;
        private CloudBlobContainer blobContainer;
        private QueueClient queueClient;

        private long currentBlockInitOffset;
        private MemoryStream toAppend = new MemoryStream(MAX_BLOCK_SIZE);

        private Stopwatch stopwatch;
        private TimeSpan MAX_CHECKPOINT_TIME = TimeSpan.FromHours(1);

        public StoreEventProcessor()
        {
            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            blobClient = storageAccount.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference("d2ctutorial");
            blobContainer.CreateIfNotExists();
            queueClient = QueueClient.CreateFromConnectionString(ServiceBusConnectionString);
        }

        Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);
            return Task.FromResult<object>(null);
        }

        Task IEventProcessor.OpenAsync(PartitionContext context)
        {
            Console.WriteLine("StoreEventProcessor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);

            if (!long.TryParse(context.Lease.Offset, out currentBlockInitOffset))
            {
                currentBlockInitOffset = 0;
            }
            stopwatch = new Stopwatch();
            stopwatch.Start();

            return Task.FromResult<object>(null);
        }

        async Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            //var queueMessages = new List<BrokeredMessage>();
            foreach (EventData eventData in messages)
            {
                byte[] data = eventData.GetBytes();
                var encodetData = Encoding.UTF8.GetString(data);
                var enqueuedTime = eventData.EnqueuedTimeUtc;
                var connectionDeviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();
                var queueMessage = new BrokeredMessage(encodetData);
                var messageId = (long) eventData.SystemProperties["SequenceNumber"];
                queueMessage.MessageId = messageId.ToString();
                queueMessage.Properties["device"] = connectionDeviceId;

                if (encodetData.StartsWith("puf"))
                {
                    var puf = SendPuf(connectionDeviceId, encodetData, enqueuedTime);
                    queueMessage.Properties["SessionId"] = puf.SmokeSessionId;
                    queueMessage.Properties["puf"] = (int)puf.Type;
                    //queueMessage.Properties["FullPuf"] = puf;
                }

                //queueMessages.Add(queueMessage);

                //WriteHighlightedMessage(string.Format("Received interactive message: {0}", messageId));



                //if (toAppend.Length + data.Length > MAX_BLOCK_SIZE || stopwatch.Elapsed > MAX_CHECKPOINT_TIME)
                //{
                //    await AppendAndCheckpoint(context);
                //}
                await AppendAndCheckpoint(context);
                await toAppend.WriteAsync(data, 0, data.Length);
               

                Console.WriteLine(string.Format("Message received.  Device: '{0}', Data: '{1}'",
                 connectionDeviceId, encodetData));
            }

            //await queueClient.SendBatchAsync(queueMessages);

            if (messages.Any())
            {
                await context.CheckpointAsync();
            }
        }

        private async Task AppendAndCheckpoint(PartitionContext context)
        {
            var blockIdString = String.Format("startSeq:{0}", currentBlockInitOffset.ToString("0000000000000000000000000"));
            var blockId = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(blockIdString));
            toAppend.Seek(0, SeekOrigin.Begin);
            byte[] md5 = MD5.Create().ComputeHash(toAppend);
            toAppend.Seek(0, SeekOrigin.Begin);

            var blobName = String.Format("iothubd2c_{0}", context.Lease.PartitionId);
            var currentBlob = blobContainer.GetBlockBlobReference(blobName);

            if (await currentBlob.ExistsAsync())
            {
                await currentBlob.PutBlockAsync(blockId, toAppend, Convert.ToBase64String(md5));
                var blockList = await currentBlob.DownloadBlockListAsync();
                var newBlockList = new List<string>(blockList.Select(b => b.Name));

                if (newBlockList.Count() > 0 && newBlockList.Last() != blockId)
                {
                    newBlockList.Add(blockId);
                    WriteHighlightedMessage(String.Format("Appending block id: {0} to blob: {1}", blockIdString, currentBlob.Name));
                }
                else
                {
                    WriteHighlightedMessage(String.Format("Overwriting block id: {0}", blockIdString));
                }
                await currentBlob.PutBlockListAsync(newBlockList);
            }
            else
            {
                await currentBlob.PutBlockAsync(blockId, toAppend, Convert.ToBase64String(md5));
                var newBlockList = new List<string>();
                newBlockList.Add(blockId);
                await currentBlob.PutBlockListAsync(newBlockList);

                WriteHighlightedMessage(String.Format("Created new blob", currentBlob.Name));
            }

            toAppend.Dispose();
            toAppend = new MemoryStream(MAX_BLOCK_SIZE);

            // checkpoint.
            await context.CheckpointAsync();
            WriteHighlightedMessage(String.Format("Checkpointed partition: {0}", context.Lease.PartitionId));

            currentBlockInitOffset = long.Parse(context.Lease.Offset);
            stopwatch.Restart();
        }

        private void WriteHighlightedMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static Puf SendPuf(string connectionDeviceId, string data, DateTime enqueuedTime)
        {
            PufType direction = ToPufType(data);
            var dataChunk = data.Split(':');
            long milis = 0;
            if (dataChunk.Length > 2)
            {
                 milis = long.Parse(dataChunk[2]);
            }

            int presure = 0;
            if (dataChunk.Length > 3)
            {
                presure = Convert.ToInt32(float.Parse(dataChunk[3],CultureInfo.InvariantCulture));
            }

            return RedisHelper.AddPuff(null,connectionDeviceId, (PufType)direction, enqueuedTime, milis,presure);
            
        }

        private static PufType ToPufType(string data)
        {
            if (data.StartsWith("puf:") && data.Length >= 5)
            {
                var a = (int)char.GetNumericValue(data[4]);

                return (PufType)a;
            }
            return PufType.Idle;
        }
    }
}
