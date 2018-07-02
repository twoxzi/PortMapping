using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Twoxzi.PortMap
{
    public class SocketBridge : BindableBase
    {
        private UInt64 sendDataLength;
        private UInt64 receiveDataLength;
        private Thread thread1;
        private Thread thread2;
        
        private Boolean canRun;


        public event Action<Exception> Stopped;
        /// <summary>
        /// 发送数据总长度
        /// </summary>
        public UInt64 SendDataLength
        {
            get
            {
                return sendDataLength;
            }

            private set
            {
                this.sendDataLength = value;
                OnPropertyChanged(nameof(SendDataLength));
            }
        }

        /// <summary>
        /// 接收数据总长度
        /// </summary>
        public UInt64 ReceiveDataLength
        {
            get
            {
                return receiveDataLength;
            }

            private set
            {
                this.receiveDataLength = value;
                OnPropertyChanged(nameof(ReceiveDataLength));
            }
        }

        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public Int32 BufferLength { get; private set; }

        private Socket from;
        private Socket to;

        /// <summary>
        /// 异步桥接Socket
        /// </summary>
        /// <param name="from_Socket"></param>
        /// <param name="to_Socket"></param>
        /// <param name="bufferLength"></param>
        public SocketBridge(Socket from_Socket, Socket to_Socket, Int32 bufferLength)
        {
            from = from_Socket;
            to = to_Socket;
            BufferLength = bufferLength;
        }

        /// <summary>
        /// 主体
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="send_complete"></param>
        private void Working(Socket s1, Socket s2, Action<Byte[], UInt32> send_complete)
        {
            try
            {
                Byte[] buff = new Byte[BufferLength];
                Int32 len = 0;
                while(canRun && (len = s1.Receive(buff)) > 0)
                {
                    s1.Send(buff, len, SocketFlags.None);
                    send_complete(buff, (uint)len);
                }
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Stopped?.Invoke(ex);
            }
            finally
            {
                canRun = false;
            }
        }

        /// <summary>
        /// 开始
        /// </summary>
        public void Start()
        {
            thread1 = new Thread(new ThreadStart(() => { Working(from, to, (buff,len) => { SendDataLength = SendDataLength + len; }); }));
            thread1.IsBackground = true;
            thread2 = new Thread(new ThreadStart(() => { Working(to, from, (buff,len) => ReceiveDataLength += len); }));
            thread2.IsBackground = true;
            canRun = true;
            thread1.Start();
            thread2.Start();
        }

        /// <summary>
        /// 结束
        /// </summary>
        /// <param name="isWait">是否等待后台线程退出</param>
        public void Stop(Boolean isWait = false)
        {
            canRun = false;
            if(canRun && isWait)
            {
                thread1.Join();
                thread2.Join();
            }
            Stopped?.Invoke(null);
        }

    }
}
