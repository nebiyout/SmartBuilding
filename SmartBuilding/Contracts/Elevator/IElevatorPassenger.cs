namespace SmartBuilding.Contracts.Elevator
{
    public interface IElevatorPassenger : IPassenger
    {
        public IElevator CalledElevator { get; set; }
    }
}
