namespace SmartBuilding.Contracts
{
    public enum MovementDirection
    {
        Idle,
        Up,
        Down
    }

    public enum ItemStatus
    {
        Available,
        OutOfService
    }

    public enum ItemType
    {
        Elevator,
        Floor,
        AC
    }

    public enum PassengerStatus
    {
        Waiting,
        OnBoard
    }
}
