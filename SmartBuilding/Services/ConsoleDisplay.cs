using SmartBuilding.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Services
{
    public class ConsoleDisplay : IDisplay
    {
        public ConsoleDisplay() { 
        }

        public void Show(string data)
        {
            Thread.Sleep(250);
            Console.WriteLine(data);
        }
    }
}
