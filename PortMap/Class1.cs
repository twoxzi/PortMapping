using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
 
namespace portmap_net
{
    /// <summary>
    /// 映射器实例状态
    /// </summary>
    sealed internal class state
    {
 
        #region Fields (5)
 
        public int _connect_cnt;
        public string _point_in;
        public string _point_out;
        public const string _print_head = "输入IP              输出IP              状态    连接数    接收/发送";
        public bool _running;
        public long _bytes_send;
        public long _bytes_recv;
 
        #endregion Fields
 
        #region Constructors (1)
 
        public state(string point_in, string point_out, bool running, int connect_cnt, int bytes_send, int bytes_recv)
        {
            _point_in = point_in;
            _point_out = point_out;
            _running = running;
            _connect_cnt = connect_cnt;
            _bytes_recv = bytes_recv;
            _bytes_send = bytes_send;
        }
 
        #endregion Constructors
 
        #region Methods (1)
 
        // Public Methods (1)
 
        public override string ToString()
        {
            return string.Format("{0}{1}{2}{3}{4}", _point_in.PadRight(20, ' '), _point_out.PadRight(20, ' '), (_running ? "运行中  " : "启动失败"), _connect_cnt.ToString().PadRight(10, ' '), Math.Round((double)_bytes_recv / 1024) + "k/" + Math.Round((double)_bytes_send / 1024) + "k");
        }
 
        #endregion Methods
    }
 
    /// <summary>
    /// 映射器线程所需数据
    /// </summary>
    internal struct work_item
    {
 
        #region Data Members (4)
 
        public int _id;
        public EndPoint _ip_in;
        public string _ip_out_host;
        public ushort _ip_out_port;
 
        #endregion Data Members
    }
 
    /// <summary>
    /// 主程序
    /// </summary>
    sealed internal class program
    {
 
        #region Fields (4)
 
        private static StringBuilder _console_buf = new StringBuilder();
        /// <summary>
        /// 程序已启动的所有映射器实例, key=id
        /// </summary>
        private static readonly Dictionary<int, state> _state_dic = new Dictionary<int, state>();
        #endregion Fields
 
        #region Methods (8)
 
        // Private Methods (8)
 
        private static void Main()
        {
            //映射器参数
            List<work_item> maps_list = new List<work_item>{
                new work_item{_id = 1, _ip_in = new IPEndPoint(IPAddress.Any,2012), _ip_out_host="222.214.218.29", _ip_out_port = 7689 }/*,
                new work_item{_id = 2, _ip_in = new IPEndPoint(IPAddress.Any,2013), _ip_out_host="www.beta-1.cn", _ip_out_port = 80 }*/
            };
 
            //启动映射器
            foreach (var map_item in maps_list)
                map_start(map_item);
 
            Console.CursorVisible = false;
            while (true)
            {
                //每2秒刷新屏幕, 显示映射器状态
                show_state();
                Thread.Sleep(2000);
            }
        }
 
        /// <summary>
        /// 启动映射器
        /// </summary>
        /// <param name="work"></param>
        private static void map_start(work_item work)
        {
            Socket sock_svr = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            bool start_error = false;
            try
            {
                sock_svr.Bind(work._ip_in);//绑定本机ip
                sock_svr.Listen(10);
                sock_svr.BeginAccept(on_local_connected, new object[] { sock_svr, work });//接受connect
            }
            catch (Exception)
            {
                start_error = true;
            }
            finally
            {
                _state_dic.Add(work._id, new state(work._ip_in.ToString(), work._ip_out_host + ":" + work._ip_out_port, !start_error, 0, 0, 0));
            }
        }
 
        /// <summary>
        /// 收到connect
        /// </summary>
        /// <param name="ar"></param>
        private static void on_local_connected(IAsyncResult ar)
        {
            object[] ar_arr = ar.AsyncState as object[];
            Socket sock_svr = ar_arr[0] as Socket;
            work_item work = (work_item)ar_arr[1];
            
 
            ++_state_dic[work._id]._connect_cnt;
            Socket sock_cli = sock_svr.EndAccept(ar);
            sock_svr.BeginAccept(on_local_connected, ar.AsyncState);
            Socket sock_cli_remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                sock_cli_remote.Connect(work._ip_out_host, work._ip_out_port);
            }
            catch (Exception)
            {
                try
                {
                    sock_cli.Shutdown(SocketShutdown.Both);
                    sock_cli_remote.Shutdown(SocketShutdown.Both);
                    sock_cli.Close();
                    sock_cli_remote.Close();
                }
                catch (Exception)
                { }
                --_state_dic[work._id]._connect_cnt;
                return;
            }
            //线程: 接受本地数据 转发至远程
            Thread t_send = new Thread(recv_and_send_caller) { IsBackground = true };
            //线程: 接受远程数据 转发至本地connect 端
            Thread t_recv = new Thread(recv_and_send_caller) { IsBackground = true };
            t_send.Start(new object[] { sock_cli, sock_cli_remote, work._id, true });
            t_recv.Start(new object[] { sock_cli_remote, sock_cli, work._id, false });
            //线程同步
            t_send.Join();
            t_recv.Join();
            //已断开, 连接数-1
            --_state_dic[work._id]._connect_cnt;
        }
 
        /// <summary>
        /// 数据转发
        /// </summary>
        /// <param name="from_sock"></param>
        /// <param name="to_sock"></param>
        /// <param name="send_complete"></param>
        private static void recv_and_send(Socket from_sock, Socket to_sock, Action<int> send_complete)
        {
            byte[] recv_buf = new byte[4096];
            int recv_len;
            while ((recv_len = from_sock.Receive(recv_buf)) > 0)
            {
                to_sock.Send(recv_buf, 0, recv_len, SocketFlags.None);
                send_complete?.Invoke(recv_len);
            }
        }
 
        private static void recv_and_send_caller(object thread_param)
        {
            object[] param_arr = thread_param as object[];
            Socket sock1 = param_arr[0] as Socket;
            Socket sock2 = param_arr[1] as Socket;
            try
            {
                recv_and_send(sock1, sock2, bytes =>
                {
                    state stat = _state_dic[(int)param_arr[2]];
                    if ((bool)param_arr[3])
                        stat._bytes_send += bytes;
                    else
                        stat._bytes_recv += bytes;
                });
            }
            catch (Exception)
            {
                try
                {
                    sock1.Shutdown(SocketShutdown.Both);
                    sock2.Shutdown(SocketShutdown.Both);
                    sock1.Close();
                    sock2.Close();
                }
                catch (Exception) { }
            }
        }
 
        private static void show_state()
        {
            StringBuilder curr_buf = new StringBuilder();
            curr_buf.AppendLine(program_ver);
            curr_buf.AppendLine(state._print_head);
            foreach (KeyValuePair<int, state> item in _state_dic)
                curr_buf.AppendLine(item.Value.ToString());
            if (_console_buf.Equals(curr_buf))
                return;
            Console.Clear();
            Console.WriteLine(curr_buf);
            _console_buf = curr_buf;
        }
 
        #endregion Methods
        private const string program_ver = @"[PortMapNet(0.1)  http://www.beta-1.cn]--------------------------------------------------";
    }
}