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

        private static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:6666"))
            {
                // initialize Logger
                Logger.InitializeLogger(LogManager.GetLogger(typeof(ServerCommunicationHub)));

                Console.Title = $"OpenNos Server Communication v1.0";
                string text = $"SERVER COMMUNICATION - PORT : 6666 by OpenNos Team";
                int offset = (Console.WindowWidth - text.Length) / 2;
                Console.WriteLine(new string('=', Console.WindowWidth));
                Console.SetCursorPosition(offset < 0 ? 0 : offset, Console.CursorTop);
                Console.WriteLine(text + "\n" +
                new string('=', Console.WindowWidth) + "\n");
                Console.ReadLine();
            }
        }

        #endregion
    }
}