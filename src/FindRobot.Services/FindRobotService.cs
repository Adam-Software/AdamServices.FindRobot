using FindRobot.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FindRobot.Services
{
    public class FindRobotService : BackgroundService
    {
        private readonly ILogger<FindRobotService> mLogger;
        private UdpClient mServer;
        private readonly byte[] mReply = Encoding.UTF8.GetBytes("pong");

        public FindRobotService(IServiceProvider serviceProvider)
        {
            mLogger = serviceProvider.GetRequiredService<ILogger<FindRobotService>>();
            ArgumentService argumentService = serviceProvider.GetService<ArgumentService>();
            ISettingsService settingsService = serviceProvider.GetRequiredService<ISettingsService>();

            int port = settingsService.Port;
            if (argumentService.Port != 0)
                port = argumentService.Port;

            mServer = new UdpClient(port);
            mLogger.LogInformation("Service running port {port}", port);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    UdpReceiveResult reciveResult = await mServer.ReceiveAsync(stoppingToken);

                    string reciveMessage = Encoding.UTF8.GetString(reciveResult.Buffer);
                    mLogger.LogTrace("Recive message {reciveMessage} from remote ep {remoteEndPoint}", reciveMessage, reciveResult.RemoteEndPoint);

                    if (reciveMessage == "ping")
                    {
                        await mServer.SendAsync(mReply, reciveResult.RemoteEndPoint, stoppingToken);
                        mLogger.LogTrace("Send {reply} to remote ep {remoteEndPoint}", Encoding.UTF8.GetString(mReply), reciveResult.RemoteEndPoint);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                mLogger.LogInformation("The service stops at the user's request");
            }
            catch (Exception ex)
            {
                mLogger.LogError("Error message {message}", ex.Message);
            }
            finally
            {
                mLogger.LogInformation("The service has been stopped");
                mServer.Close();
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (mServer != null)
                {
                    mServer.Dispose();
                    mServer = null;
                }
            }
        }
    }
}
