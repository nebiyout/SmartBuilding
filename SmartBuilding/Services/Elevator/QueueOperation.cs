using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using System.Threading.Tasks;

namespace SmartBuilding.Services.Elevator
{
    public class QueueOperation : IOperation<IElevator>
    {
        private readonly IElevator elevator;
        private readonly IElevatorPassenger passenger;

        public QueueOperation(IElevator elevator, IElevatorPassenger passenger)
        {
            this.elevator = elevator;
            this.passenger = passenger;
        }

        public IElevator Execute()
        {
            elevator.Passengers.Add(passenger);
            return elevator;
        }
    }
}
