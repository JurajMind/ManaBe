using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using smartHookah.Controllers;
using smartHookah.Helpers;
using smartHookah.Hubs;
using smartHookah.Models;
using smartHookah.Models.Redis;
using smartHookah.Support;
using smartHookahCommon;

namespace ProcessDeviceToCloudMessages
{
    internal class StoreEventProcessor : IEventProcessor
    {
        private const int MAX_BLOCK_SIZE = 4*1024*1024;
        public static string StorageConnectionString;
        public static string ServiceBusConnectionString;
        private readonly CloudBlobClient blobClient;
        private readonly CloudBlobContainer blobContainer;

        private long currentBlockInitOffset;
        private readonly TimeSpan MAX_CHECKPOINT_TIME = TimeSpan.FromHours(1);
        private QueueClient queueClient;

        private Stopwatch stopwatch;
        private MemoryStream toAppend = new MemoryStream(MAX_BLOCK_SIZE);

        public StoreEventProcessor()
        {
            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            blobClient = storageAccount.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference("d2ctutorial");
            blobContainer.CreateIfNotExists();
            queueClient = QueueClient.CreateFromConnectionString(ServiceBusConnectionString);
        }

        private IHubContext ClientContext => GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();

        Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId,
                reason);
            return Task.FromResult<object>(null);
        }

        Task IEventProcessor.OpenAsync(PartitionContext context)
        {
            Console.WriteLine("StoreEventProcessor initialized.  Partition: '{0}', Offset: '{1}'",
                context.Lease.PartitionId, context.Lease.Offset);

            if (!long.TryParse(context.Lease.Offset, out currentBlockInitOffset))
                currentBlockInitOffset = 0;
            stopwatch = new Stopwatch();
            stopwatch.Start();

            return Task.FromResult<object>(null);
        }

        async Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            var queueMessages = new List<BrokeredMessage>();
            foreach (var eventData in messages)
            {
                var data = eventData.GetBytes();
                var encodetData = Encoding.UTF8.GetString(data);
                var enqueuedTime = eventData.EnqueuedTimeUtc;
                var connectionDeviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();


                if (encodetData.StartsWith("puf"))
                {
                    var puf = SendPuf(connectionDeviceId, encodetData, enqueuedTime);
                    OnPuf(connectionDeviceId, puf);
                }
                else
                {
                    OnOther(connectionDeviceId, encodetData);
                }


                if ((toAppend.Length + data.Length > MAX_BLOCK_SIZE) || (stopwatch.Elapsed > MAX_CHECKPOINT_TIME))
                    await AppendAndCheckpoint(context);
                await toAppend.WriteAsync(data, 0, data.Length);


                Console.WriteLine("Message received.  Device: '{0}', Data: '{1}'", connectionDeviceId, encodetData);
            }

            //await queueClient.SendBatchAsync(queueMessages);

            if (messages.Any())
                await context.CheckpointAsync();
        }

        private async Task AppendAndCheckpoint(PartitionContext context)
        {
            var blockIdString = string.Format("startSeq:{0}",
                currentBlockInitOffset.ToString("0000000000000000000000000"));
            var blockId = Convert.ToBase64String(Encoding.ASCII.GetBytes(blockIdString));
            toAppend.Seek(0, SeekOrigin.Begin);
            var md5 = MD5.Create().ComputeHash(toAppend);
            toAppend.Seek(0, SeekOrigin.Begin);

            var blobName = string.Format("iothubd2c_{0}", context.Lease.PartitionId);
            var currentBlob = blobContainer.GetBlockBlobReference(blobName);

            if (await currentBlob.ExistsAsync())
            {
                await currentBlob.PutBlockAsync(blockId, toAppend, Convert.ToBase64String(md5));
                var blockList = await currentBlob.DownloadBlockListAsync();
                var newBlockList = new List<string>(blockList.Select(b => b.Name));

                if ((newBlockList.Count() > 0) && (newBlockList.Last() != blockId))
                {
                    newBlockList.Add(blockId);
                    WriteHighlightedMessage(string.Format("Appending block id: {0} to blob: {1}", blockIdString,
                        currentBlob.Name));
                }
                else
                {
                    WriteHighlightedMessage(string.Format("Overwriting block id: {0}", blockIdString));
                }
                await currentBlob.PutBlockListAsync(newBlockList);
            }
            else
            {
                await currentBlob.PutBlockAsync(blockId, toAppend, Convert.ToBase64String(md5));
                var newBlockList = new List<string>();
                newBlockList.Add(blockId);
                await currentBlob.PutBlockListAsync(newBlockList);

                WriteHighlightedMessage(string.Format("Created new blob", currentBlob.Name));
            }

            toAppend.Dispose();
            toAppend = new MemoryStream(MAX_BLOCK_SIZE);

            // checkpoint.
            await context.CheckpointAsync();
            WriteHighlightedMessage(string.Format("Checkpointed partition: {0}", context.Lease.PartitionId));

            currentBlockInitOffset = long.Parse(context.Lease.Offset);
            stopwatch.Restart();
        }

        public void OnPuf(string deviceId, Puf puf)
        {
            var pufType = puf.Type;
            ClientContext.Clients.Group(puf.SmokeSessionId).pufChange(pufType.ToWebStateString(), pufType.ToGraphData());
            UpdateStistics(deviceId, puf);
        }

        public void OnOther(string deviceId, string msg)
        {
            try
            {
                var content = msg;
                if (content.StartsWith("connect"))
                {
                  
                    var version = "1.0.0";
                    if (content.Length > 7)
                        version = content.Substring(8);


                    //DeviceControlController.InitDevice(deviceId, version);
                    using (var db = new SmartHookahContext())

                    {
                        SmokeSessionController.InitSmokeSession(db, deviceId);
                    }
                    using (var redis = RedisHelper.redisManager.GetClient())
                    {
                        redis.As<string>().Lists["Connect:" + deviceId].Add($"{DateTime.Now}");
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }


        private void UpdateStistics(string deviceId, Puf puf)
        {
            using (var redis = RedisHelper.redisManager.GetClient())
            {
                var session = puf.SmokeSessionId;
                var ds = redis.As<DynamicSmokeStatistic>()["DS:" + session];

                if ((ds == null) || (ds.LastFullUpdate < DateTime.Now.AddMinutes(-5)))
                {
                    if (ds == null)
                        ds = new DynamicSmokeStatistic();
                    ds.FullUpdate(redis, session);
                }
                else
                {
                    if (puf != null)
                        ds.Update(puf, session, deviceId);
                }

                redis.As<DynamicSmokeStatistic>()["DS:" + session] = ds;

                var ownDs = new
                {
                    pufCount = ds.PufCount,
                    lastPuf = ds.LastPufDuration.ToString(@"s\.fff"),
                    lastPufTime = ds.LastPufTime.AddHours(-1).ToString("dd-MM-yyyy HH:mm:ss"),
                    smokeDuration = ds.TotalSmokeTime.ToString(@"hh\:mm\:ss"),
                    longestPuf = ds.LongestPuf.ToString(@"s\.fff"),
                    start = ds.Start.ToString("dd-MM-yyyy HH:mm:ss"),
                    duration = ((DateTime.UtcNow - ds.Start).ToString(@"hh\:mm\:ss")),
                    longestPufMilis = ds.LongestPuf.TotalMilliseconds
                };

                ClientContext.Clients.Group(session).updateStats(ownDs);
                ClientContext.Clients.Group(deviceId).updateStats(deviceId,ownDs);
            }
        }


        private void WriteHighlightedMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static Puf SendPuf(string connectionDeviceId, string data, DateTime enqueuedTime)
        {
            var direction = ToPufType(data);
            var dataChunk = data.Split(':');
            long milis = 0;
            if (dataChunk.Length > 2)
                milis = long.Parse(dataChunk[2]);

            var presure = 0;
            if (dataChunk.Length > 3)
                presure = Convert.ToInt32(float.Parse(dataChunk[3], CultureInfo.InvariantCulture));

            return RedisHelper.AddPuff(null, connectionDeviceId, direction, enqueuedTime, milis, presure);
        }

        private static PufType ToPufType(string data)
        {
            if (data.StartsWith("puf:") && (data.Length >= 5))
            {
                var a = (int) char.GetNumericValue(data[4]);

                return (PufType) a;
            }
            return PufType.Idle;
        }
    }
}