using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Core.Dto;
using SmartBuilding.Services.Display;
using SmartBuilding.Services.Elevator.Notification;
using SmartBuilding.Utils;
using SmartBuilding.Utils.PubSub;
using System;
using System.Threading.Tasks.Dataflow;
using static SmartBuilding.Utils.CommonHelper;

namespace SmartBuilding.Services.Elevator
{
    public class MoveOperation : IOperation<IElevator>
    {
        private readonly IElevator elevator;
        private readonly IEnumerable<IFloor> floors;
        private readonly int minFloor;
        private readonly int maxFloor;
        private static Object obj = new object();

        public MoveOperation(IElevator elevator)
        {
            floors = BuildingHelper.GetItems<IFloor>();
            _ = floors ?? throw new ArgumentNullException(nameof(floors));
            this.elevator = elevator;

            minFloor = floors.Min(i => i.FloorNo);
            maxFloor = floors.Min(i => i.FloorNo);
            
            NotificationManager<ElevatorUpdateDto>.Subscribe(new MovementNotification(CommonHelper.GetDisplay));
        }


        public IElevator Execute()
        {
            RunMoveTask();
            //Task.Run(() => RunMoveTask()).ConfigureAwait(false);

            return elevator;
        }
        

        private void RunMoveTask()
        {
            lock (obj)
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
                        int farCaller = GetFarCaller();
                        int farPassanger = GetFarPassenger();

                        int farJobIndex = Math.Max(farCaller, farPassanger);
                        int startJobIndex = elevator.CurrentFloor.FloorNo;
                        while (startJobIndex <= farJobIndex)
                        {
                            OffLoadPassengers();
                            LoadPassengers();
                            BroadCast();

                            var nextFloor = floors.FirstOrDefault(i => i.FloorNo == startJobIndex + 1);
                            if (nextFloor != null && startJobIndex + 1 <= farJobIndex)
                                elevator.CurrentFloor = nextFloor;

                            startJobIndex++;
                        }

                        elevator.Direction = MovementDirection.Down;
                    }
                    else if (elevator.Direction == MovementDirection.Down)
                    {
                        int nearCaller = GetNearCaller();
                        int nearPassanger = GetNearPassenger();

                        int nearJobIndex = Math.Min(nearCaller, nearPassanger);
                        int startJobIndex = elevator.CurrentFloor.FloorNo;
                        while (startJobIndex >= nearJobIndex)
                        {
                            OffLoadPassengers();
                            LoadPassengers();
                            BroadCast();

                            var prevFloor = floors.FirstOrDefault(i => i.FloorNo == startJobIndex - 1);
                            if (prevFloor != null && startJobIndex - 1 >= nearJobIndex)
                                elevator.CurrentFloor = prevFloor;

                            startJobIndex--;
                        }
                        elevator.Direction = MovementDirection.Up;
                    }

                }
            }
        }

        private void BroadCast()
        {
            NotificationManager<ElevatorUpdateDto>.Notify(new ElevatorUpdateDto()
            {
                FloorNo = elevator.CurrentFloor.FloorNo,
                Direction = elevator.Direction.ToString(),
                ElevatorName = elevator.ItemId,
                OnBoardPassengers = elevator.Passengers.Count(i => i.ToFloor != null || (i.ToFloor == null && i.Waiting == false))
            });
        }

        private int GetFarCaller()
        {
            int farCaller = int.MinValue;
            var callers = elevator.Passengers.Where(i => i.ToFloor == null && i.FromFloor.FloorNo >= elevator.CurrentFloor.FloorNo && i.Waiting == true);
            if (callers.Any())
                farCaller = callers.Max(i => i.FromFloor.FloorNo);

            return farCaller;
        }

        private int GetFarPassenger()
        {
            int farPassenger = int.MinValue;
            var passengers = elevator.Passengers.Where(i => i.ToFloor != null && i.ToFloor.FloorNo >= elevator.CurrentFloor.FloorNo && i.Waiting == false);
            if (passengers.Any())
                farPassenger = passengers.Max(i => i.ToFloor.FloorNo);

            return farPassenger;
        }

        private int GetNearCaller()
        {
            int nearCaller = int.MaxValue;
            var callers = elevator.Passengers.Where(i => i.ToFloor == null && i.FromFloor.FloorNo <= elevator.CurrentFloor.FloorNo && i.Waiting == true);
            if (callers.Any())
                nearCaller = callers.Min(i => i.FromFloor.FloorNo);

            return nearCaller;
        }

        private int GetNearPassenger()
        {
            int nearPassenger = int.MaxValue;
            var passengers = elevator.Passengers.Where(i => i.ToFloor != null && i.ToFloor.FloorNo <= elevator.CurrentFloor.FloorNo && i.Waiting == false);
            if (passengers.Any())
                nearPassenger = passengers.Min(i => i.ToFloor.FloorNo);

            return nearPassenger;
        }

        private void OffLoadPassengers()
        {
            Func<IElevatorPassenger, bool> funcDepartingPassengers = i => i.ToFloor != null && i.Waiting == false
            && i.ToFloor.FloorNo == elevator.CurrentFloor.FloorNo;

            new UnloadOperation(elevator, funcDepartingPassengers).Execute();
        }

        private void LoadPassengers()
        {
           new LoadOperation(elevator, floors).Execute();
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
