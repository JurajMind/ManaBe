using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.AspNet.SignalR.Client;

namespace KeyEmulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainAsync();
        }

        static async Task MainAsync()
        {
            try
            {

                var hubConnection = new HubConnection("http://app.manapipes.com/");
                //hubConnection.TraceLevel = TraceLevels.All;
                //hubConnection.TraceWriter = Console.Out;
                IHubProxy hubProxy = hubConnection.CreateHubProxy("SmokeSessionHub");
                hubProxy.On("pufChange", data =>
                {
                    Console.WriteLine("Incoming data: {0} {1}", data.name, data.message);
                });
                ServicePointManager.DefaultConnectionLimit = 10;
                await hubConnection.Start();
                await hubProxy.Invoke("joinSession", "6O1HT");

            }
            catch (Exception ex)
            {

            }
        }

    }
}
