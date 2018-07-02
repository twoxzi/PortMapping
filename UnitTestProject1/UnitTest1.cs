using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Tcp);
            receiver.Bind(new IPEndPoint(IPAddress.Any, 23001));
            receiver.Listen(10);

        }
    }
}
