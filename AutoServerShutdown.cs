using AMP;
using AMP.Data;
using AMP.DedicatedServer;
using AMP.Logging;
using AMP.Network.Data;
using Netamite.Server.Data;
using System;
using System.Threading;

namespace AutoServerShutdown {
    public class AutoServerShutdown : AMP_Plugin {

        public override string NAME => "AutoShutdown";
        public override string AUTHOR => "Adammantium";
        public override string VERSION => "1.0.0";

        private static long ms_to_connect = 5 * 60 * 1000;
        AutoShutdownConfig config;

        private Thread autoShutdown;
        public override void OnStart() {
            config = (AutoShutdownConfig) GetConfig();
            if(config.shutdownDelay >= 0) {
                ms_to_connect = config.shutdownDelay * 1000;
            }

            autoShutdown = new Thread(() => {
                Thread.Sleep(2000);
                while(ms_to_connect > 0) {
                    if(ModManager.serverInstance == null) return;

                    try {
                        if(ModManager.serverInstance.connectedClients > 0) {
                            Log.Debug("AutoShutdown", "Connected in time.");
                            return;
                        }
                    }catch(NullReferenceException) { }

                    if(ms_to_connect % 30000 == 0) {
                        Log.Debug("AutoShutdown", $"Time left: {ms_to_connect / 1000}s");
                    }
                    
                    Thread.Sleep(1000);
                    ms_to_connect -= 1000;
                }

                ShutdownServer();
            });
            autoShutdown.Name = "AutoShutdown";
            autoShutdown.Start();
        }

        public override void OnStop() {
            autoShutdown.Abort();
        }

        public override void OnPlayerQuit(ClientData client) {
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
