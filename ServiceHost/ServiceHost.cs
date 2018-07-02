using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace Twoxzi.PortMap
{
    public partial class ServiceHost : ServiceBase
    {
        public ServiceHost()
        {
            InitializeComponent();
        }

        private List<Socket> connectSocket;
        private List<NetNode> clients;

        public List<NetNode> Clients
        {
            get
            {
                if(clients == null)
                {
                    clients = new List<NetNode>();
                }
                return clients;
            }

            set
            {
                this.clients = value;
            }
        }

        protected override void OnStart(string[] args)
        {
            // 侦听请求时使用TCP协议
            Socket listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            // 编写TCP和配置文件里的端口
            listener.Bind(new IPEndPoint(IPAddress.Any,(ushort) Properties.Settings.Default["ServerListenPort"]));
            // 开始侦听
            listener.Listen(10);
            // 接入请求
            connectSocket = new List<Socket>();
            while(true)
            {
                Socket child = listener.Accept();
                Thread t = new Thread(OnConnected) { IsBackground = true };
                t.Start(child);
                //listener.ReceiveFrom()
                
            }

        }


        private void OnConnected(Object sourceSocket)
        {
            Socket socket = sourceSocket as Socket;
            if(socket == null)
            {
                return;
            }
            connectSocket.Add(socket);

            Byte[] temp = new Byte[1024];
            
            
        }

        protected override void OnStop()
        {
        }
    }
}
