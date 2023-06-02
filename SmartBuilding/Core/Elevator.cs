using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;

namespace SmartBuilding.Core
{
    public class Elevator : IElevator
    {
        public Elevator(string itemId, IFloor currentFloor, int maxWeight)
        {
            if (currentFloor == null)
                throw new ArgumentNullException();

            if (maxWeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxWeight));

            ItemId = itemId;
            CurrentFloor = currentFloor;
            MaxPassengerLimit = maxWeight;
            Direction = MoveType.Idle;
            Passengers = new List<IElevatorPassenger>();
        }

        public string ItemId { get; set; }

        public MoveType Direction { get; set; } //changes during move, load and unload operations

        public int MaxPassengerLimit { get; set; } //changes during load and unload operation

        public IFloor CurrentFloor { get; set; } //changes during move operation

        public IBuilding? Building { get; set; }

        public List<IElevatorPassenger> Passengers { get; set; }//changes during queue and unload operation

        public ItemStatus ItemStatus { get; set; }

        public void SetElevatorStatus(int minFloor, int maxFloor)
        {
            if (CurrentFloor.FloorNo == minFloor)
                Direction = MoveType.Up;

            else if (CurrentFloor.FloorNo == maxFloor)
                Direction = MoveType.Down;
        }

        public void ResetStatus()
        {
            Direction = MoveType.Idle;
        }
    }
}
