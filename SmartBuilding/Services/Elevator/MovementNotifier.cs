using SmartBuilding.Contracts;
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
        private static IList<ElevatorMovement> elevatorMovements= new List<ElevatorMovement>();

        public void OnNext(ElevatorMovement value)
        {
            if (elevatorMovements.Any(i => i.ElevatorName == value.ElevatorName &&
            i.CurrentFloor == value.CurrentFloor &&
            i.Direction == value.Direction &&
            i.OnBoardPassengers == value.OnBoardPassengers))
                return;

            Thread.Sleep(200);

            Console.WriteLine($"Elevator : {value.ElevatorName}  |  Floor No.: {value.CurrentFloor.FloorNo}  |  Direction: {value.Direction.ToString()}  |  Passengers : " + value.OnBoardPassengers);
            elevatorMovements.Add(value);
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
