using Microsoft.Azure.Devices.Client;
using System;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DeviceEmulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        static DeviceClient deviceClient;
        static string iotHubUri = "smartHookah.azure-devices.net";
        static string deviceKey = "NQeKlguPIMevEN/YSUtZJy9TajUz9yRWfo7p/C9C4OQ=";
        private static DateTime DeviceStart;
        public MainWindow()
        {
            InitializeComponent();
            DeviceStart = DateTime.Now;
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("emulator", deviceKey));
        }


        private async void SendPufMessages(PufType pufType)
        {

            var milis = System.Convert.ToInt64((DateTime.Now - DeviceStart).TotalMilliseconds);
            var msg = $"puf:{(int)pufType}:{milis}";


            if (pufType == PufType.Idle)
            {
                msg = msg + ":100,";
            }
            var message = new Message(Encoding.ASCII.GetBytes(msg));
            message.MessageId = new Random().Next(290, 100000000).ToString();
            //await deviceClient.SendEventAsync(message);
            Post(msg);
            MsgBox.Items.Add(msg);

            MsgBox.SelectedIndex = MsgBox.Items.Count - 1;
            MsgBox.ScrollIntoView(MsgBox.SelectedItem);
            Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, msg);



        }

        private void Post(string msg)
        {
            try
            {
                StringContent queryString = new StringContent(msg);
                client.PostAsync(Url.Text, queryString);
            }
            catch (Exception e)
            {

            }

        }

        private void pufButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SendPufMessages(PufType.In);
        }

        private void pufButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SendPufMessages(PufType.Idle);
        }

        private void blowButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SendPufMessages(PufType.Out);
        }

        private void blowButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SendPufMessages(PufType.Idle);
        }
    }

    public enum PufType
    {
        In = 1, Out = 2, Idle = 0
    }

}
