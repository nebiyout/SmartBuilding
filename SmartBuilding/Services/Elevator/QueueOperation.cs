using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;

namespace SmartBuilding.Services.Elevator
{
    public class QueueOperation : IOperation<IElevator>
    {
        private readonly IElevator elevator;
        private readonly IList<IElevatorPassenger> passengers;

        public QueueOperation(IElevator elevator, IList<IElevatorPassenger> passengers)
        {
            this.elevator = elevator;
            this.passengers = passengers;
        }

        public IElevator Execute()
        {
            foreach (IElevatorPassenger passenger in passengers)
                elevator.Passengers.Add(passenger);

            return elevator;
        }
    }
}
