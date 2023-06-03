using SmartBuilding.Contracts;
using SmartBuilding.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Services.Elevator.Notification
{
    public class LoadingNotification : IObserver<LoadingDto>
    {
        private readonly IDisplay display;

        public LoadingNotification(IDisplay display)
        {
            this.display = display;
        }

        public void OnNext(LoadingDto value)
        {
            string data = $"Elevator : {value.ElevatorName}  |  Floor No.: {value.FloorNo}  |  Operation: {value.Operation}  {value.OnBoardPassengers}  Passengers";
            display.Show(data);
        }

        public void OnError(Exception error)
        {

        }

        public void OnCompleted()
        {
        }
    }
}
