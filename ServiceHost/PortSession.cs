using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace Twoxzi.PortMap
{
    public class PortSession : AppSession<PortSession>
    {
        public NetNode RemoteNode { get; internal set; }


        public new PortServer AppServer
        {
            get
            {
                return (PortServer)base.AppServer;
            }
        }

        protected override void OnSessionStarted()
        {

        }
    }
}
