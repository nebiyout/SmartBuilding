using SmartBuilding.Contracts.Floor;

namespace SmartBuilding.Contracts
{
    public interface IPassenger
    {
        IFloor FromFloor { get; set; }

        IFloor? ToFloor { get; set; }

        PassengerStatus Status { get; set; }

        MovementDirection Direction { get; set; }
    }
  }
