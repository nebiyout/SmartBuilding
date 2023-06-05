using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Core.Dto;
using SmartBuilding.Services.Elevator.Notification;
using SmartBuilding.Utils.PubSub;
using System.Linq;

namespace SmartBuilding.Services.Elevator
{
    public class UnloadOperation : IOperation<IElevator>
    {
        private readonly IElevator elevator;
        private Func<IElevatorPassenger, bool> criteria;
        public bool HasUnloaded { get; set; }

        public UnloadOperation(IElevator elevator, Func<IElevatorPassenger, bool> criteria)
        {
            this.elevator = elevator;
            this.criteria = criteria;
        }
        
        /// <summary>
        /// unload passengers
        /// </summary>
        /// <returns></returns>
        public IElevator Execute()
        {
            Predicate<IElevatorPassenger> predepartingPassengers = new Predicate<IElevatorPassenger>(criteria);

            if (elevator.Passengers.Any(criteria))
            {
                HasUnloaded = true;
                elevator.Passengers.RemoveAll(predepartingPassengers);
            }

            return elevator;
        }
    }
}
