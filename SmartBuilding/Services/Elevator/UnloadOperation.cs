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

        public UnloadOperation(IElevator elevator, Func<IElevatorPassenger, bool> criteria)
        {
            this.elevator = elevator;
            this.criteria = criteria;
            NotificationManager<LoadingDto>.Subscribe(new LoadingNotification());
        }
                
        public async Task<IElevator> ExecuteAsync()
        {
            Predicate<IElevatorPassenger> predepartingPassengers = new Predicate<IElevatorPassenger>(criteria);

            if (elevator.Passengers.Any(criteria))
            {
                Broadcast(elevator.Passengers.Count(criteria));
                elevator.Passengers.RemoveAll(predepartingPassengers);
            }

            return await Task.FromResult(elevator);
        }

        private void Broadcast(int totalPassengers)
        {
            NotificationManager<LoadingDto>.Notify(new LoadingDto()
            {
                ElevatorName = elevator.ItemId,
                FloorNo = elevator.CurrentFloor.FloorNo,
                Operation = "Unloading Passengers",
                OnBoardPassengers = totalPassengers
            });
        }
    }
}
