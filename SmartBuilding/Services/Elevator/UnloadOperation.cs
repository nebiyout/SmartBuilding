using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;

namespace SmartBuilding.Services.Elevator
{
    public class UnloadOperation : IOperation<IElevator>
    {
        private readonly IElevator elevator;
        private readonly IList<IElevatorPassenger> passengers;

        public UnloadOperation(IElevator elevator, IList<IElevatorPassenger> passengers)
        {
            this.elevator = elevator;
            this.passengers = passengers;
        }

        public async Task<IElevator> ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
