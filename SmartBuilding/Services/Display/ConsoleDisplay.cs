﻿using SmartBuilding.Contracts;
using SmartBuilding.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartBuilding.Services.Display
{
    public class ConsoleDisplay : IDisplay
    {
        private StreamWriter writer;
        private NamedPipeClientStream client;

        public ConsoleDisplay()
        {
            client = new NamedPipeClientStream("PipeName");
            writer = new StreamWriter(client);
        }

        public void Send(string data)
        {
            try
            {
                if (!client.IsConnected)
                    client.Connect();

                StreamWriter writer = new StreamWriter(client);
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
