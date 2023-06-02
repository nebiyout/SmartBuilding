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
        private static IList<LoadingDto> elevatorMovements = new List<LoadingDto>();

        public void OnNext(LoadingDto value)
        {
            Thread.Sleep(100);

            Console.WriteLine($"Elevator : {value.ElevatorName}  |  Floor No.: {value.FloorNo}  |  Operation: {value.Operation}  {value.OnBoardPassengers}  Passengers");
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
