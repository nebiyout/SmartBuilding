using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;

namespace SmartBuilding.Core
{
    public class Elevator : IElevator
    {
        public Elevator(string itemId, IFloor currentFloor, int maxPassengerLimit)
        {
            if (currentFloor == null)
                throw new ArgumentNullException();

            if (maxPassengerLimit <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxPassengerLimit));

            ItemId = itemId;
            CurrentFloor = currentFloor;
            MaxPassengerLimit = maxPassengerLimit;
            Direction = MoveDirection.Idle;
            Passengers = new List<IElevatorPassenger>();
        }

        public Elevator(string itemId, IFloor currentFloor, int maxPassengerLimit, MoveDirection direction) : 
            this(itemId, currentFloor, maxPassengerLimit)
        {
            Direction = direction;
        }

        public string ItemId { get; set; }

        public MoveDirection Direction { get; set; } //changes during move, load and unload operations

        public int MaxPassengerLimit { get; set; } //changes during load and unload operation

        public IFloor CurrentFloor { get; set; } //changes during move operation

        public IBuilding? Building { get; set; }

        public List<IElevatorPassenger> Passengers { get; set; }//changes during queue and unload operation

        public ItemStatus ItemStatus { get; set; }

        public void SetElevatorStatus(int minFloor, int maxFloor)
        {
            if (CurrentFloor.FloorNo == minFloor)
                Direction = MoveDirection.Up;

            else if (CurrentFloor.FloorNo == maxFloor)
                Direction = MoveDirection.Down;
        }

        public void ResetStatus()
        {
            Direction = MoveDirection.Idle;
        }
    }
}
