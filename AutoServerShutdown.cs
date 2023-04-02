using AMP;
using AMP.DedicatedServer;
using AMP.Logging;
using Netamite.Server.Data;
using System;
using System.Threading;

namespace AutoServerShutdown {
    public class AutoServerShutdown : AMP_Plugin {

        public override string NAME => "AutoShutdown";
        public override string AUTHOR => "Adammantium";
        public override string VERSION => "1.0.0";

        private const long ms_to_connect = 5 * 60 * 1000;

        private Thread autoShutdown;
        public override void OnStart() {
            autoShutdown = new Thread(() => {
                long time_til_shutdown = ms_to_connect;
                Thread.Sleep(2000);
                while(time_til_shutdown > 0) {
                    if(ModManager.serverInstance == null) return;

                    try {
                        if(ModManager.serverInstance.connectedClients > 0) {
                            Log.Debug("AutoShutdown", "Connected in time.");
                            return;
                        }
                    }catch(NullReferenceException) { }

                    if(time_til_shutdown % 30000 == 0) {
                        Log.Debug("AutoShutdown", $"Time left: {time_til_shutdown / 1000}s");
                    }
                    
                    Thread.Sleep(1000);
                    time_til_shutdown -= 1000;
                }

                ShutdownServer();
            });
            autoShutdown.Name = "AutoShutdown";
            autoShutdown.Start();
        }

        public override void OnStop() {
            autoShutdown.Abort();
        }

        public override void OnPlayerQuit(ClientInformation client) {
            if(ModManager.serverInstance.connectedClients == 0) {
                ShutdownServer();
            }
        }

        private void ShutdownServer() {
            Log.Debug("AutoShutdown", "Stopping server...");

            Thread shuttingDown = new Thread(() => {
                CommandHandler.GetCommandHandler("stop").Process(new string[0]);
                Environment.Exit(0);
                autoShutdown.Abort();
            });
            shuttingDown.Name = "AutoShutdown Shutdown";
            shuttingDown.Start();
        }
    }
}
