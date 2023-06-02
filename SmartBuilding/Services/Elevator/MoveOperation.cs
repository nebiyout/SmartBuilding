using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Core.Dto;
using SmartBuilding.Services.Elevator.Notification;
using SmartBuilding.Utils;
using SmartBuilding.Utils.PubSub;
using System.Threading.Tasks.Dataflow;

namespace SmartBuilding.Services.Elevator
{
    public class MoveOperation : IOperation<IElevator>
    {
        private readonly IElevator elevator;
        private readonly IEnumerable<IFloor> floors;
        private readonly int minFloor;
        private readonly int maxFloor;
        private readonly NotificationManager<ElevatorUpdateDto> notificationManager;

        public MoveOperation(IElevator elevator)
        {
            floors = BuildingHelper.GetItems<IFloor>();
            _ = floors ?? throw new ArgumentNullException(nameof(floors));
            this.elevator = elevator;

            minFloor = floors.Min(i => i.FloorNo);
            maxFloor = floors.Min(i => i.FloorNo);

            notificationManager = new NotificationManager<ElevatorUpdateDto>();
            notificationManager.Subscribe(new MovementNotification());
        }


        public async Task<IElevator> ExecuteAsync()
        {
            while (true)
            {
                if (elevator.Passengers.All(i => i.Waiting == false) && elevator.Passengers.All(i => i.Waiting == false && i.ToFloor == null))
                {
                    elevator.ResetStatus();
                    break;
                }

                InitiateElevatorStatus();

                if (elevator.Direction == MovementDirection.Up)
                {
                    int callersMax = await GetCallerMaxValueAsync();
                    int passengersMax = await GetPassengerMaxValueAsync();

                    int maxJobIndex = Math.Max(callersMax, passengersMax);
                    int startJobIndex = elevator.CurrentFloor.FloorNo;
                    while (startJobIndex <= maxJobIndex)
                    {
                        await OffLoadPassengers();
                        await LoadPassengers();
                        BroadCast();

                        var nextFloor = floors.FirstOrDefault(i => i.FloorNo == startJobIndex + 1);
                        if (nextFloor != null && startJobIndex + 1 <= maxJobIndex)
                            elevator.CurrentFloor = nextFloor;

                        startJobIndex++;
                    }

                    elevator.Direction = MovementDirection.Down;
                }
                else if (elevator.Direction == MovementDirection.Down)
                {
                    int callersMin = await GetCallerMinValueAsync();
                    int passengersMin = await GetPassengerMinValueAsync();

                    int minJobIndex = Math.Min(callersMin, passengersMin);
                    int startJobIndex = elevator.CurrentFloor.FloorNo;
                    while (startJobIndex >= minJobIndex)
                    {
                        await OffLoadPassengers();
                        await LoadPassengers();
                        BroadCast();

                        var prevFloor = floors.FirstOrDefault(i => i.FloorNo == startJobIndex - 1);
                        if (prevFloor != null && startJobIndex - 1 >= minJobIndex)
                            elevator.CurrentFloor = prevFloor;

                        startJobIndex--;
                    }
                    elevator.Direction = MovementDirection.Up;
                }
            }

            return await Task.FromResult(elevator);
        }

        private void BroadCast()
        {
            notificationManager.Notify(new ElevatorUpdateDto()
            {
                FloorNo = elevator.CurrentFloor.FloorNo,
                Direction = elevator.Direction.ToString(),
                ElevatorName = elevator.ItemId,
                OnBoardPassengers = elevator.Passengers.Count(i => i.ToFloor != null || (i.ToFloor == null && i.Waiting == false))
            });
        }

        private async Task<int> GetCallerMaxValueAsync()
        {
            int callersMax = int.MinValue;
            var callers = elevator.Passengers.Where(i => i.ToFloor == null && i.FromFloor.FloorNo >= elevator.CurrentFloor.FloorNo && i.Waiting == true);
            if (callers.Any())
                callersMax = callers.Max(i => i.FromFloor.FloorNo);

            return await Task.FromResult(callersMax);
        }

        private async Task<int> GetPassengerMaxValueAsync()
        {
            int passengersMax = int.MinValue;
            var passengers = elevator.Passengers.Where(i => i.ToFloor != null && i.ToFloor.FloorNo >= elevator.CurrentFloor.FloorNo && i.Waiting == false);
            if (passengers.Any())
                passengersMax = passengers.Max(i => i.ToFloor.FloorNo);

            return await Task.FromResult(passengersMax);
        }

        private async Task<int> GetCallerMinValueAsync()
        {
            int callersMin = int.MaxValue;
            var callers = elevator.Passengers.Where(i => i.ToFloor == null && i.FromFloor.FloorNo <= elevator.CurrentFloor.FloorNo && i.Waiting == true);
            if (callers.Any())
                callersMin = callers.Min(i => i.FromFloor.FloorNo);

            return await Task.FromResult(callersMin);
        }

        private async Task<int> GetPassengerMinValueAsync()
        {
            int passengersMin = int.MaxValue;
            var passengers = elevator.Passengers.Where(i => i.ToFloor != null && i.ToFloor.FloorNo <= elevator.CurrentFloor.FloorNo && i.Waiting == false);
            if (passengers.Any())
                passengersMin = passengers.Min(i => i.ToFloor.FloorNo);

            return await Task.FromResult(passengersMin);
        }

        private async Task OffLoadPassengers()
        {
            Func<IElevatorPassenger, bool> funcDepartingPassengers = i => i.ToFloor != null && i.Waiting == false
            && i.ToFloor.FloorNo == elevator.CurrentFloor.FloorNo;

            await new UnloadOperation(elevator, funcDepartingPassengers).ExecuteAsync();
        }

        private async Task LoadPassengers()
        {
            await new LoadOperation(elevator).ExecuteAsync();
        }

        private void InitiateElevatorStatus()
        {
            if (elevator.Direction == MovementDirection.Idle && elevator.Passengers.Any())
            {
                var upperPassangers = elevator.Passengers
                    .Where(i => i.FromFloor.FloorNo > elevator.CurrentFloor.FloorNo);

                if (upperPassangers.Any())
                    elevator.Direction = MovementDirection.Up;
                else
                    elevator.Direction = MovementDirection.Down;
            }
        }
    }
}
