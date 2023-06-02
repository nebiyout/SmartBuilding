using SmartBuilding.Contracts.Floor;

namespace SmartBuilding.Contracts.Elevator
{
    public interface IElevator : IBuildingItem
    {
        MovementDirection Direction { get; set; }

        IFloor CurrentFloor { get; set; }

        List<IElevatorPassenger> Passengers { get; set; }

        int MaxPassengerLimit { get; set; }

        void SetElevatorStatus(int minFloor, int maxFloor);

        void ResetStatus();
    }
}
