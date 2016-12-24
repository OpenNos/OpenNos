// Copyright (c) .NET Foundation. All rights reserved. Licensed under the Apache License, Version
// 2.0. See License.txt in the project root for license information.

using log4net;
using Microsoft.Owin.Hosting;
using OpenNos.Core;
using System;

namespace OpenNos.WebApi.SelfHost
{
    public class Program
    {
        #region Methods

        private static void Main()
        {
            using (WebApp.Start<Startup>("http://localhost:6666"))
            {
                // initialize Logger
                Logger.InitializeLogger(LogManager.GetLogger(typeof(ServerCommunicationHub)));

                Console.Title = "OpenNos Server Communication v1.0";
                const string text = "SERVER COMMUNICATION - PORT : 6666 by OpenNos Team";
                int offset = Console.WindowWidth / 2 + text.Length / 2;
                string separator = new string('=', Console.WindowWidth);
                Console.WriteLine(separator + string.Format("{0," + offset + "}", text) + "\n" + separator);
                Console.ReadLine();
            }
        }

        #endregion
    }
}