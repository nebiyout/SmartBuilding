using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Core;

namespace SmartBuilding.Services.Elevator
{
    public class CallOperation : IOperation<IElevator>
    {
        private readonly IEnumerable<IElevator> elevators;
        private readonly IFloor callerFloor;
        private readonly MovementDirection callerMove;

        public CallOperation(IEnumerable<IElevator> elevators, IFloor callerFloor, MovementDirection callerMove)
        {
            _ = elevators ?? throw new ArgumentNullException(nameof(elevators));
            _ = callerFloor ?? throw new ArgumentNullException(nameof(callerFloor));

            this.elevators = elevators;
            this.callerFloor = callerFloor;
            this.callerMove = callerMove;
        }

        public async Task<IElevator> ExecuteAsync()
        {
            IElevator? closestElevator = await FindClosestElevatorAsync();

            _ = closestElevator ?? throw new ArgumentNullException("No closest available elevator");

            return closestElevator;
        }

        private async Task<IElevator?> FindClosestElevatorAsync()
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
                    if (callerMove == MovementDirection.Up)
                    {
                        //check if the elevator is at lower floor than the caller floor.
                        if (elevator.CurrentFloor.FloorNo < callerFloor.FloorNo)
                        {
                            distance = Math.Abs(callerFloor.FloorNo - elevator.CurrentFloor.FloorNo);
                        }
                        else if (elevator.CurrentFloor.FloorNo > callerFloor.FloorNo)
                        {
                            distance = await GetMinDistance(elevator, callerFloor);
                        }
                    }
                    else if (callerMove == MovementDirection.Down)
                    {
                        distance = await GetMinDistance(elevator, callerFloor);
                    }
                }
                else if (elevator.Direction == MovementDirection.Down)
                {
                    if (callerMove == MovementDirection.Down)
                    {
                        //check if the elevator is at lower floor than the caller floor.
                        if (elevator.CurrentFloor.FloorNo < callerFloor.FloorNo)
                        {
                            distance = await GetMinDistance(elevator, callerFloor);
                        }
                        else if (elevator.CurrentFloor.FloorNo > callerFloor.FloorNo)
                        {
                            distance = Math.Abs(elevator.CurrentFloor.FloorNo - callerFloor.FloorNo);
                        }
                    }
                    else if (callerMove == MovementDirection.Up)
                    {
                        distance = await GetMinDistance(elevator, callerFloor);
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
        private async Task<int> GetMinDistance(IElevator elevator, IFloor callerFloor)
        {
            int minDistance = int.MaxValue;

            var passangers = elevator.Passengers;

            if (!passangers.Any())
            {
                minDistance = Math.Abs(elevator.CurrentFloor.FloorNo - callerFloor.FloorNo);
            }
            else
            {
                var last = 0;
                var first = 0;

                var lastOrder = passangers.Where(i => i.ToFloor != null);
                if (lastOrder.Any())
                    last = lastOrder.Max(i => i.ToFloor.FloorNo);

                var firstOrder = passangers.Where(i => i.ToFloor != null);

                if (firstOrder.Any())
                    first = firstOrder.Min(i => i.ToFloor.FloorNo);

                if (elevator.Direction == MovementDirection.Up)
                {
                    if (callerMove == MovementDirection.Up)
                    {
                        minDistance = Math.Abs(last - elevator.CurrentFloor.FloorNo);
                        minDistance += Math.Abs(last - first);
                        minDistance += Math.Abs(callerFloor.FloorNo - first);
                    }
                    else if (callerMove == MovementDirection.Down)
                    {
                        minDistance = Math.Abs(last - elevator.CurrentFloor.FloorNo);
                        minDistance += Math.Abs(last - callerFloor.FloorNo);
                    }
                }
                else if (elevator.Direction == MovementDirection.Down)
                {
                    if (callerMove == MovementDirection.Down)
                    {
                        minDistance = Math.Abs(elevator.CurrentFloor.FloorNo - first);
                        minDistance += Math.Abs(last - first);
                        minDistance += Math.Abs(last - callerFloor.FloorNo);
                    }
                    else if (callerMove == MovementDirection.Up)
                    {
                        minDistance = Math.Abs(elevator.CurrentFloor.FloorNo - first);
                        minDistance += Math.Abs(callerFloor.FloorNo - first);
                    }
                }
            }

            return await Task.FromResult(minDistance);
        }
    }
}
