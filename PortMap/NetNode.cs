using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Twoxzi.PortMap
{
    /// <summary>
    /// Client是指当前服务下
    /// </summary>
    /// 

    public class NetNode:ICloneable
    {
        private String host;
        private String ip;
        private String nodeName;
        private UInt16 port;
        private Int32 nodeId;
        private List<NetNode> nodeList;
        private Boolean isSelf;
        private NetNode parent;
        /// <summary>
        /// 客户端名称
        /// </summary>
        [XmlAttribute]
        public String NodeName
        {
            get
            {
                return nodeName;
            }

            set
            {
                this.nodeName = value;
            }
        }
        /// <summary>
        /// 端口
        /// </summary>
        [XmlAttribute]
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
        /// 主机名
        /// </summary>
        [XmlAttribute]
        public String Host
        {
            get
            {
                return host;
            }

            set
            {
                this.host = value;
            }
        }

        /// <summary>
        /// 客户端ID
        /// </summary>
        [XmlAttribute]
        public Int32 NodeId
        {
            get
            {
                return nodeId;
            }

            set
            {
                this.nodeId = value;
            }
        }

        /// <summary>
        /// 子节点
        /// </summary>
        [XmlElement(ElementName ="NetNode")]
        public List<NetNode> NodeList
        {
            get
            {
                return nodeList;
            }

            set
            {
                this.nodeList = value;
            }
        }
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

        public Object Clone()
        {
            return null;
        }
    }
}
