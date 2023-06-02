using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;

namespace SmartBuilding.Services.Elevator
{
    public class LoadOperation : IOperation<IElevator>
    {
        private readonly IElevator elevator;

        public LoadOperation(IElevator elevator)
        {
            this.elevator = elevator;
        }

        public Task<IElevator> ExecuteAsync()
        {
            //passengers who are going in to the elevator 
            var passengersGotToElevator = elevator.Passengers
               .Where(i => i.ToFloor == null && i.Waiting == true
               && i.FromFloor.FloorNo == elevator.CurrentFloor.FloorNo
               && elevator.Direction == i.Direction
               && elevator.ItemId == i.CalledElevator.ItemId);
            
            foreach (IElevatorPassenger passenger in passengersGotToElevator)
                passenger.Waiting = false;

            return Task.FromResult(elevator);
        }
    }
}
