using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Twoxzi.PortMap
{
    /// <summary>
    /// 数据包
    /// </summary>
    public class Packet
    {
        private String targetIp;
        private UInt16 port;
        private Packet nextNode;
        private PacketType packetType;
        private Boolean isRelay;

        /// <summary>
        /// 目录IP
        /// </summary>
        public String TargetIp
        {
            get
            {
                return targetIp;
            }

            set
            {
                this.targetIp = value;
            }
        }

        /// <summary>
        /// 目标端口
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

        /// <summary>
        /// 下个节点
        /// </summary>
        public Packet NextNode
        {
            get
            {
                return nextNode;
            }

            set
            {
                this.nextNode = value;
            }
        }

        /// <summary>
        /// 数据包类型
        /// </summary>
        public PacketType PacketType
        {
            get
            {
                return packetType;
            }

            set
            {
                this.packetType = value;
            }
        }

        /// <summary>
        /// 是否转发
        /// </summary>
        public Boolean IsRelay
        {
            get
            {
                return isRelay;
            }

            set
            {
                this.isRelay = value;
            }
        }
    }
}
