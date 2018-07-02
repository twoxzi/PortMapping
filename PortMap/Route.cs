using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Twoxzi.PortMap
{
    /// <summary>
    /// 转发路由
    /// </summary>
    public class Route
    {
        /// <summary>
        /// 目标地址
        /// </summary>
        public String TargetAddress { get; private set; }
        /// <summary>
        /// 目标端口
        /// </summary>
        public Int32 TargetPort { get; private set; }
        /// <summary>
        /// 本地端口
        /// </summary>
        public Int32 LocalPort { get; private set; }
        /// <summary>
        /// 缓存大小
        /// </summary>
        public Int32 BuffSize { get; private set; }

        /// <summary>
        /// 运行状态
        /// </summary>
        public RouteState State { get; private set; }
        /// <summary>
        /// 路由列表
        /// </summary>
        public static Dictionary<Int32, Route> RouteDic { get { return routeDic; } }
        /// <summary>
        /// 转发器
        /// </summary>
        public List<SocketBridge> WorkerList { get { return workerList; } }

        public AddressFamily AddressFamilySetting { get; set; }

        private Int32 backlog = 100;

        private static Dictionary<Int32, Route> routeDic = new Dictionary<int, Route>();

        private List<SocketBridge> workerList = new List<SocketBridge>();

        private Socket acceptSocket = null;
        

        private Route()
        {
            
        }

        public static Route CreateOrGetRoute(String targetAddress, Int32 targetPort, Int32 localPort, Int32 buffSize)
        {
            Route route;
            lock(routeDic)
            {
                if(!routeDic.TryGetValue(localPort, out route))
                {
                    route = new Route() { TargetAddress = targetAddress, TargetPort = targetPort, LocalPort = localPort, BuffSize = buffSize };

                }
                return route;
            }

        }

        
        /// <summary>
        /// 测试链接
        /// </summary>
        /// <returns></returns>
        public AddressFamily TestConnection()
        {
            //AddressFamily temp = AddressFamily.InterNetwork;
            //SocketException ipv4Exception = null;
            //SocketException ipv6Exception = null;

            if(AddressFamilySetting == AddressFamily.InterNetwork)
            {
                TestConnection(AddressFamilySetting);

            }
            else if(AddressFamilySetting == AddressFamily.InterNetworkV6)
            {
                TestConnection(AddressFamilySetting);
            }
            else
            {
                try
                {
                    TestConnection(AddressFamily.InterNetwork);
                    return AddressFamily.InterNetwork;
                }
                catch(Exception ex) { }

                try
                {
                    TestConnection(AddressFamily.InterNetworkV6);
                    return AddressFamily.InterNetworkV6;
                }
                catch(Exception ex) { }
            }
            return AddressFamily.Unspecified;
        }

        public void TestConnection(AddressFamily addressFamily)
        {
            try
            {
                Socket remote = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
                remote.Connect(TargetAddress, TargetPort);
            }
            catch(SocketException ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                throw ex;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            Start(backlog);
        }

        public void Start(Int32 backlog)
        {

            AddressFamily temp = AddressFamily.Unspecified;
            if((temp = TestConnection()) != AddressFamily.Unspecified)
            {
                AddressFamilySetting = temp;
                State = RouteState.Running;
                if(acceptSocket != null)
                {
                    try
                    {
                        acceptSocket.Close();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message + ex.StackTrace);
                       // throw;
                    }
                }
                acceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                acceptSocket.Bind(new System.Net.IPEndPoint(IPAddress.Any, LocalPort));
                acceptSocket.Listen(backlog);
                acceptSocket.BeginAccept(onLocalConnected, new Object[] { acceptSocket });
            }
        }



        public void Stop()
        {
            lock(this)
            {
                State = RouteState.Waiting;
                acceptSocket.Close();
            }
        }
        private void onLocalConnected(IAsyncResult ar)
        {

            Object[] objs = ar.AsyncState as Object[];
            Socket tempSocket = objs[0] as Socket;
            Socket local = tempSocket.EndAccept(ar);
            
            try
            {
                tempSocket.BeginAccept(onLocalConnected, ar.AsyncState);
                Console.WriteLine("In onLocalConnected");
                Socket remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                remote.Connect(TargetAddress, TargetPort);

                SocketBridge worker = new SocketBridge(local, remote, BuffSize);// { parent = this };
                worker.Stopped += (ex) => { WorkerList.Remove(worker);  Console.WriteLine(ex == null ? "正常退出" : ex.Message + ex.StackTrace); };
                worker.Start();
                WorkerList.Add(worker);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

    }
}
