using SmartBuilding.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Services.Display
{
    public class DebugDisplay : IDisplay
    {
        public void Show(string data)
        {
            Thread.Sleep(250);
            System.Diagnostics.Debug.WriteLine(data);
        }
    }
}
