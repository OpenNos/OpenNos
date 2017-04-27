using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.ScsServices.Service;
using OpenNos.Master.Interface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenNos.Master.Server
{

    class Program
    {
        private static ManualResetEvent run = new ManualResetEvent(true);
        private static IScsServiceApplication _server;
        private static CommunicationService _communicationService;

        static void Main(string[] args)
        {
            string ipAddress = "127.0.0.1";
            int port = 6969;

            _server = ScsServiceBuilder.CreateService(new ScsTcpEndPoint(ipAddress, port));
            _communicationService = new CommunicationService();


            _server.AddService<ICommunicationService, CommunicationService>(_communicationService);

            _server.Start();
        }
    }
}
