using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Core;
using SmartBuilding.Core.Dto;
using SmartBuilding.Services.Elevator.Notification;
using SmartBuilding.Utils.PubSub;
using System.Threading.Tasks.Dataflow;

namespace SmartBuilding.Services.Elevator
{
    public class LoadOperation : IOperation<IElevator>
    {
        private readonly IElevator elevator;
        private readonly IEnumerable<IFloor> floors;

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
                //if(elevator.Passengers.Count(i=>i.Waiting == false) + passengersGotToElevator.Count() > elevator.MaxPassengerLimit)
                //{
                //    Console.
                //}
               
                foreach (IElevatorPassenger passenger in passengersGotToElevator)
                {
                    passenger.Waiting = false;
                }

            }
            
            return elevator;
        }
    }
}
