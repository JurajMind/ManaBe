using Microsoft.AspNet.SignalR.Client;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

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
