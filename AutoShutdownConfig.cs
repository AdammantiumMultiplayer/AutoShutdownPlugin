using AMP.DedicatedServer.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServerShutdown {
    internal class AutoShutdownConfig : PluginConfig {

        public int shutdownDelay = 300;

    }
}
