﻿using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;

namespace SmartBuilding.Core
{
    public class ElevatorPassenger : IElevatorPassenger
    {
        public ElevatorPassenger(IElevator calledElevator, IFloor fromFloor, IFloor? toFloor, MovementDirection moveDirection)
        {
            CalledElevator = calledElevator;
            FromFloor = fromFloor;
            ToFloor = toFloor;
            Direction = moveDirection;
        }

        public IElevator CalledElevator { get; set; }

        public IFloor FromFloor { get; set; }

        public IFloor? ToFloor { get; set; } //will be set during load operation

        public PassengerStatus Status { get; set; } = PassengerStatus.Waiting;

        public MovementDirection Direction { get; set; }
    }
}
