using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;

namespace SmartBuilding.Services.Elevator
{
    public class LoadOperation : IOperation<IElevator>
    {
        private readonly IElevator elevator;
        private readonly IEnumerable<IElevatorPassenger> passengers;

        public LoadOperation(IElevator elevator, IEnumerable<IElevatorPassenger> passengers)
        {
            this.elevator = elevator;
            this.passengers = passengers;
        }

        public Task<IElevator> ExecuteAsync()
        {
            foreach (IElevatorPassenger passenger in passengers)
            {
                passenger.Waiting = false;
                elevator.Passengers.Add(passenger);
            }

            return Task.FromResult(elevator);
        }
    }
}
