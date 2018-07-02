using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Web;

namespace PortMapWcfService
{
    public class PackData
    {
        private String ip;
        private UInt16 port;
        private ProtocolType protocolType;

        /// <summary>
        /// Ip
        /// </summary>
        public String Ip
        {
            get
            {
                return ip;
            }

            set
            {
                this.ip = value;
            }
        }

        /// <summary>
        /// 端口
        /// </summary>
        public UInt16 Port
        {
            get
            {
                return port;
            }

            set
            {
                this.port = value;
            }
        }

        public ProtocolType ProtocolType
        {
            get
            {
                return protocolType;
            }

            set
            {
                if(value == ProtocolType.Tcp || value == ProtocolType.Udp)
                {
                    this.protocolType = value;
                }
                else { throw new Exception("目前只支持TCP和UDP协议."); }
            }
        }
    }
}