using SmartBuilding.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Services.Elevator
{
    public class MovementNotifier : IObserver<ElevatorMovement>
    {
        public void OnNext(ElevatorMovement value)
        {
            Thread.Sleep(200);
            Console.WriteLine($"Elevator : {value.ElevatorName}  |  Floor No.: {value.CurrentFloor.FloorNo}  |  Direction: {value.Direction.ToString()}  |  Passengers : " + value.OnBoardPassengers);
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
