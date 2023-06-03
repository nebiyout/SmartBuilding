using SmartBuilding.Contracts;
using SmartBuilding.Services.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Utils
{
    public static class CommonHelper
    {
        public static IDisplay? display;

        public static IDisplay GetDisplay
        {
            get
            {
                if (display == null)
                    display = Activator.CreateInstance<ConsoleDisplay>();

                return display;
            }
        }
    }
}
