using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Services.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Utils
{
    public class CommonHelper
    {
        public delegate void LoadEventHandler(IElevator elevator, IList<IFloor> floors,IElevatorPassenger passenger);

        public delegate void MessageEventHandler(string message);

        public static int  MinElevatorLimit => 4;

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
