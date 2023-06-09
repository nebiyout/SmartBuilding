﻿using SmartBuilding.Contracts;
using SmartBuilding.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Services.Elevator.Notification
{
    public class MovementNotification : IObserver<ElevatorUpdateDto>
    {
        private readonly IDisplay display;

        public MovementNotification(IDisplay display)
        {
            this.display = display;
        }

        public void OnNext(ElevatorUpdateDto value)
        {
            Thread.Sleep(150);
            string data = $"Elevator : {value.ElevatorName}  |  Floor No.: {value.FloorNo}  |  Direction: {value.Direction}  |  Passengers : " + value.OnBoardPassengers;
            display.Send(data);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}
