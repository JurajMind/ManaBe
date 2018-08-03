namespace smartHookah
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    using log4net;

    using Microsoft.Extensions.Logging;

    using Ninja.WebSockets;

    public class WebSocketServer : IDisposable
    {
        private TcpListener listener;
        private bool isDisposed;
        private readonly ILog logger = LogManager.GetLogger(typeof(EmailService));
        private readonly IWebSocketServerFactory webSocketServerFactory;

        private readonly ILoggerFactory loggerFactory;

        private readonly HashSet<string> supportedSubProtocols;

        public WebSocketServer(IWebSocketServerFactory webSocketServerFactory, ILoggerFactory loggerFactory, IList<string> supportedSubProtocols = null)
        {
            this.webSocketServerFactory = webSocketServerFactory;
            this.loggerFactory = loggerFactory;

            this.supportedSubProtocols = new HashSet<string>(supportedSubProtocols ?? new string[0]);
        }

        private void ProcessTcpClient(TcpClient tcpClient)
        {
            Task.Run(() => this.ProcessTcpClientAsync(tcpClient));
        }

        private string GetSubProtocol(IList<string> requestedSubProtocols)
        {
            foreach (string subProtocol in requestedSubProtocols)
            {
                // match the first sub protocol that we support (the client should pass the most preferable sub protocols first)
                if (this.supportedSubProtocols.Contains(subProtocol))
                {
                    this.logger.Info($"Http header has requested sub protocol {subProtocol} which is supported");

                    return subProtocol;
                }
            }

            if (requestedSubProtocols.Count > 0)
            {
                this.logger.Warn($"Http header has requested the following sub protocols: {string.Join(", ", requestedSubProtocols)}. There are no supported protocols configured that match.");
            }

            return null;
        }

        private async Task ProcessTcpClientAsync(TcpClient tcpClient)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            try
            {
                if (this.isDisposed)
                {
                    return;
                }

                // this worker thread stays alive until either of the following happens:
                // Client sends a close conection request OR
                // An unhandled exception is thrown OR
                // The server is disposed
                this.logger.Info("Server: Connection opened. Reading Http header from stream");

                // get a secure or insecure stream
                Stream stream = tcpClient.GetStream();
                WebSocketHttpContext context = await this.webSocketServerFactory.ReadHttpHeaderFromStreamAsync(stream);
                if (context.IsWebSocketRequest)
                {
                    string subProtocol = this.GetSubProtocol(context.WebSocketRequestedProtocols);
                    var options = new WebSocketServerOptions() { KeepAliveInterval = TimeSpan.FromSeconds(30), SubProtocol = subProtocol };
                    this.logger.Info("Http header has requested an upgrade to Web Socket protocol. Negotiating Web Socket handshake");

                    WebSocket webSocket = await this.webSocketServerFactory.AcceptWebSocketAsync(context, options);

                    this.logger.Info("Web Socket handshake response sent. Stream ready.");
                    await this.RespondToWebSocketRequestAsync(webSocket, source.Token);
                }
                else
                {
                    this.logger.Info("Http header contains no web socket upgrade request. Ignoring");
                }

                this.logger.Info("Server: Connection closed");
            }
            catch (ObjectDisposedException)
            {
                // do nothing. This will be thrown if the Listener has been stopped
            }
            catch (Exception ex)
            {
                this.logger.Error(ex.ToString());
            }
            finally
            {
                try
                {
                    tcpClient.Client.Close();
                    tcpClient.Close();
                    source.Cancel();
                }
                catch (Exception ex)
                {
                    this.logger.Error($"Failed to close TCP connection: {ex}");
                }
            }
        }

        public async Task RespondToWebSocketRequestAsync(WebSocket webSocket, CancellationToken token)
        {
            const int BufferLen = 4 * 1024 * 1024; // 4MB
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[BufferLen]);

            while (true)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    this.logger.Info($"Client initiated close. Status: {result.CloseStatus} Description: {result.CloseStatusDescription}");
                    break;
                }

                if (result.Count > BufferLen)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig,
                        $"Web socket frame cannot exceed buffer size of {BufferLen:#,##0} bytes. Send multiple frames instead.",
                        token);
                    break;
                }

                // just echo the message back to the client
                ArraySegment<byte> toSend = new ArraySegment<byte>(buffer.Array, buffer.Offset, result.Count);
                await webSocket.SendAsync(toSend, WebSocketMessageType.Binary, true, token);
            }
        }

        public async Task Listen(int port)
        {
            try
            {
                IPAddress localAddress = IPAddress.Any;
                this.listener = new TcpListener(localAddress, port);
                this.listener.Start();
                this.logger.Info($"Server started listening on port {port}");
                while (true)
                {
                    TcpClient tcpClient = await this.listener.AcceptTcpClientAsync();
                    this.ProcessTcpClient(tcpClient);
                }
            }
            catch (SocketException ex)
            {
                string message = string.Format("Error listening on port {0}. Make sure IIS or another application is not running and consuming your port.", port);
                throw new Exception(message, ex);
            }
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;

                // safely attempt to shut down the listener
                try
                {
                    if (this.listener != null)
                    {
                        if (this.listener.Server != null)
                        {
                            this.listener.Server.Close();
                        }

                        this.listener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex.ToString());
                }

                this.logger.Info("Web Server disposed");
            }
        }
    }
}