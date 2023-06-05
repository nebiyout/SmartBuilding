using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Core;
using SmartBuilding.Core.Dto;
using SmartBuilding.Services.Elevator.Notification;
using SmartBuilding.Utils.PubSub;
using System;
using System.Threading.Tasks.Dataflow;
using static SmartBuilding.Utils.CommonHelper;

namespace SmartBuilding.Services.Elevator
{
    public class LoadOperation : IOperation<IElevator>
    {
        private readonly IElevator elevator;
        private readonly IEnumerable<IFloor> floors;
        public static event LoadEventHandler loadEvent;
        public static event MessageEventHandler messageEvent;
        public bool MaxedOut { get; set; }

        public LoadOperation(IElevator elevator, IEnumerable<IFloor> floors)
        {
            this.elevator = elevator;
            this.floors = floors;
        }

        /// <summary>
        /// load passengers
        /// </summary>
        /// <returns></returns>
        public IElevator Execute()
        {
            var passengersGotToElevator = elevator.Passengers
               .Where(i => i.ToFloor == null && i.Waiting == true
               && i.FromFloor.FloorNo == elevator.CurrentFloor.FloorNo
               && elevator.Direction == i.Direction
               && elevator.ItemId == i.CalledElevator.ItemId);

            if (passengersGotToElevator.Any())
            {
                int totalPassengers = passengersGotToElevator.Count() + elevator.Passengers.Count(i => i.Waiting == false);
                if (totalPassengers > elevator.MaxPassengerLimit)
                {
                    MaxedOut = true;
                    string message = "Elevator (" + elevator.ItemId + ") has reached its maximum passanger limit.";
                    messageEvent?.Invoke(message);
                }
                else
                {
                    MaxedOut = false;
                    foreach (IElevatorPassenger passenger in passengersGotToElevator)
                    {
                        passenger.Waiting = false;
                        loadEvent?.Invoke(elevator, floors.ToList(), passenger);
                    }
                }
            }

            return elevator;
        }
    }
}
