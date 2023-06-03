﻿using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Core;

namespace SmartBuilding.Services.Elevator
{
    public class CallOperation : IOperation<IElevator>
    {
        private readonly IEnumerable<IElevator> elevators;
        private readonly IFloor callerFloor;
        private readonly MovementDirection callerDirection;

        public CallOperation(IEnumerable<IElevator> elevators, IFloor callerFloor, MovementDirection callerDirection)
        {
            _ = elevators ?? throw new ArgumentNullException(nameof(elevators));
            _ = callerFloor ?? throw new ArgumentNullException(nameof(callerFloor));

            this.elevators = elevators;
            this.callerFloor = callerFloor;
            this.callerDirection = callerDirection;
        }

        public IFloor CallerFloor => callerFloor;

        public MovementDirection CallerDirection => callerDirection;

        public IElevator Execute()
        {
            IElevator? closestElevator = FindClosestElevator();

            _ = closestElevator ?? throw new ArgumentNullException("No closest available elevator");

            return closestElevator;
        }

        private IElevator? FindClosestElevator()
        {
            IElevator? closestElevator;

            var avalableElevators = elevators.Where(i => i.Direction == MovementDirection.Idle ||
             i.Direction == MovementDirection.Up || i.Direction == MovementDirection.Down);

            if (!avalableElevators.Any())
                throw new NullReferenceException("No available elevator.");

            //if the elevator is at the caller's current floor and its in stopped state
            closestElevator = avalableElevators.FirstOrDefault(i => i.CurrentFloor.FloorNo == callerFloor.FloorNo);
            if (closestElevator != null)
                return closestElevator;

            int minDistance = int.MaxValue;
            int distance;

            foreach (IElevator elevator in avalableElevators)
            {
                distance = 0;

                //check if the elevetor is idle
                if (elevator.Direction == MovementDirection.Idle)
                {
                    //calulate the distance by substracting elevator floor from the caller's floor
                    distance = Math.Abs(callerFloor.FloorNo - elevator.CurrentFloor.FloorNo);
                }
                //check if the elevator is going up
                else if (elevator.Direction == MovementDirection.Up)
                {
                    if (callerDirection == MovementDirection.Up)
                    {
                        //check if the elevator is at lower floor than the caller floor.
                        if (elevator.CurrentFloor.FloorNo < callerFloor.FloorNo)
                        {
                            distance = Math.Abs(callerFloor.FloorNo - elevator.CurrentFloor.FloorNo);
                        }
                        else if (elevator.CurrentFloor.FloorNo > callerFloor.FloorNo)
                        {
                            distance = GetMinDistance(elevator, callerFloor);
                        }
                    }
                    else if (callerDirection == MovementDirection.Down)
                    {
                        distance = GetMinDistance(elevator, callerFloor);
                    }
                }
                else if (elevator.Direction == MovementDirection.Down)
                {
                    if (callerDirection == MovementDirection.Down)
                    {
                        //check if the elevator is at lower floor than the caller floor.
                        if (elevator.CurrentFloor.FloorNo < callerFloor.FloorNo)
                        {
                            distance = GetMinDistance(elevator, callerFloor);
                        }
                        else if (elevator.CurrentFloor.FloorNo > callerFloor.FloorNo)
                        {
                            distance = Math.Abs(elevator.CurrentFloor.FloorNo - callerFloor.FloorNo);
                        }
                    }
                    else if (callerDirection == MovementDirection.Up)
                    {
                        distance = GetMinDistance(elevator, callerFloor);
                    }
                }


                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestElevator = elevator;
                }
            }

            return closestElevator;
        }
        /// <summary>
        /// get minimum distance from the queue
        /// </summary>
        /// <param name="elevator"></param>
        /// <param name="callerFloor"></param>
        /// <returns></returns>
        private int GetMinDistance(IElevator elevator, IFloor callerFloor)
        {
            int minDistance = int.MaxValue;

            var passangers = elevator.Passengers;

            if (!passangers.Any())
            {
                minDistance = Math.Abs(elevator.CurrentFloor.FloorNo - callerFloor.FloorNo);
            }
            else
            {
                var near = 0; 
                var far = 0;

                var farOrder = passangers.Where(i => i.ToFloor != null);
                if (farOrder.Any())
                    far = farOrder.Max(i => i.ToFloor.FloorNo);

                var nearOrder = passangers.Where(i => i.ToFloor != null);

                if (nearOrder.Any())
                    near = nearOrder.Min(i => i.ToFloor.FloorNo);

                if (elevator.Direction == MovementDirection.Up)
                {
                    if (callerDirection == MovementDirection.Up)
                    {
                        minDistance = Math.Abs(far - elevator.CurrentFloor.FloorNo);
                        minDistance += Math.Abs(far - near);
                        minDistance += Math.Abs(callerFloor.FloorNo - near);
                    }
                    else if (callerDirection == MovementDirection.Down)
                    {
                        minDistance = Math.Abs(far - elevator.CurrentFloor.FloorNo);
                        minDistance += Math.Abs(far - callerFloor.FloorNo);
                    }
                }
                else if (elevator.Direction == MovementDirection.Down)
                {
                    if (callerDirection == MovementDirection.Down)
                    {
                        minDistance = Math.Abs(elevator.CurrentFloor.FloorNo - near);
                        minDistance += Math.Abs(far - near);
                        minDistance += Math.Abs(far - callerFloor.FloorNo);
                    }
                    else if (callerDirection == MovementDirection.Up)
                    {
                        minDistance = Math.Abs(elevator.CurrentFloor.FloorNo - near);
                        minDistance += Math.Abs(callerFloor.FloorNo - near);
                    }
                }
            }

            return minDistance;
        }
    }
}
