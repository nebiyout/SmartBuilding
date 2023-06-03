using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Floor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Core.Dto
{
    public class ElevatorUpdateDto
    {
        public string ElevatorName { get; set; } = string.Empty;

        public string Direction { get; set; } = string.Empty;

        public int FloorNo { get; set; }

        public int OnBoardPassengers { get; set; }
    }


}
