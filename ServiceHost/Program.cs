using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace Twoxzi.PortMap
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {

            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new ServiceHost()
            //};
            //ServiceBase.Run(ServicesToRun);

            // PortMappingHost host = new PortMappingHost("127.0.0.1", 23001, 23002);
            //Route host = Route.CreateOrGetRoute("127.0.0.1", 3389, 33891, 20432);
            //host.Run();

            Route route = Route.CreateOrGetRoute("sodaforwork", 1433, 14332,4096);

            route.Start();

            while(true)
            {
                Console.WriteLine(route.WorkerList.Count);
                Thread.Sleep(2000);

            }

            

        }
    }
}
