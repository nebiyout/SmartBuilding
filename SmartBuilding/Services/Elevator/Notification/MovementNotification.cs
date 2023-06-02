using SmartBuilding.Contracts;
using SmartBuilding.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Services.Elevator.Notification
{
    public class MovementNotification : IObserver<ElevatorUpdateDto>
    {
        private static IList<ElevatorUpdateDto> elevatorMovements = new List<ElevatorUpdateDto>();

        public void OnNext(ElevatorUpdateDto value)
        {
            Thread.Sleep(100);

            Console.WriteLine($"Elevator : {value.ElevatorName}  |  Floor No.: {value.FloorNo}  |  Direction: {value.Direction}  |  Passengers : " + value.OnBoardPassengers);
        }

        public void OnError(Exception error)
        {
            Console.WriteLine($"An error occurred: {error.Message}");
        }

        public void OnCompleted()
        {
            Console.WriteLine("Completed");
        }
    }
}
