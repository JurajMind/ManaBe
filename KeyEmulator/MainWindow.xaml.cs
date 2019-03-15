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
        private IHubProxy hubProxy;
        private HubConnection hubConnection;
        public MainWindow()
        {
            InitializeComponent();
            MainAsync();
        }

        async Task MainAsync()
        {
            try
            {

                hubConnection = new HubConnection("http://app.manapipes.com/");
                //hubConnection.TraceLevel = TraceLevels.All;
                //hubConnection.TraceWriter = Console.Out;
                hubProxy = hubConnection.CreateHubProxy("SmokeSessionHub");
                hubProxy.On("pufChange", data =>
                {
                    SendKeys.Send("1");
                });
                ServicePointManager.DefaultConnectionLimit = 10;
                await hubConnection.Start();
              

            }
            catch (Exception ex)
            {

            }
        }

        async Task JoinSession(string smokeSessionId)
        {
            await this.Dispatcher.Invoke(async () =>
            {
                await hubProxy.Invoke("joinSession", smokeSessionId);
            });
           
        }

        private async void JoinSession_Click(object sender, RoutedEventArgs e)
        {
            await hubProxy.Invoke("joinSession", sessionId.Text);
        }

    }


}
