using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Core.Dto
{
    public class LoadingDto
    {
        public string ElevatorName { get; set; } = string.Empty;

        public int FloorNo { get; set; }

        public string Operation { get; set; } = string.Empty;

        public int OnBoardPassengers { get; set; }
    }
}
