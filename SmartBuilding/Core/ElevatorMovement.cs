using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Floor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Core
{
    public class ElevatorMovement
    {
        public string ElevatorName { get; set; }

        public MoveDirection Direction { get; set; }

        public IFloor CurrentFloor { get; set; } 

        public int OnBoardPassengers { get; set; }
    }
}
