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
        private readonly MovementDirection callerDirection;

        public CallOperation(IEnumerable<IElevator> elevators, IFloor callerFloor, MovementDirection callerDirection)
        {
            _ = elevators ?? throw new ArgumentNullException(nameof(elevators));
            _ = callerFloor ?? throw new ArgumentNullException(nameof(callerFloor));
           
            if(!elevators.Any())
                throw new ArgumentException("No elevator(s).");

            this.elevators = elevators;
            this.callerFloor = callerFloor;
            this.callerDirection = callerDirection;
        }

        public IFloor CallerFloor => callerFloor;

        public MovementDirection CallerDirection => callerDirection;

        public IElevator Execute()
        {
            IElevator? closestElevator = FindClosestElevator();

            _ = closestElevator ?? throw new ArgumentException("No closest available elevator");

            return closestElevator;
        }

        private IElevator? FindClosestElevator()
        {
            IElevator? closestElevator;

            var avalableElevators = elevators.Where(i => i.ItemStatus == ItemStatus.Available);

            if (!avalableElevators.Any())
                throw new ArgumentException("No available elevator(s).");

            //if the elevator is at the caller's current floor and its in idle state
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

                int farCaller = GetFarCaller(elevator);
                int farPassanger = GetFarPassenger(elevator);
                far = Math.Max(farCaller, farPassanger);

                int nearCaller = GetNearCaller(elevator);
                int nearPassanger = GetNearPassenger(elevator);
                near = Math.Min(nearCaller, nearPassanger);

                //var farOrder = passangers.Where(i => i.ToFloor != null);
                //if (farOrder.Any())
                //    far = farOrder.Max(i => i.ToFloor.FloorNo);

                //var nearOrder = passangers.Where(i => i.ToFloor != null);

                //if (nearOrder.Any())
                //    near = nearOrder.Min(i => i.ToFloor.FloorNo);

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

        private int GetFarCaller(IElevator elevator)
        {
            int farCaller = int.MinValue;
            var callers = elevator.Passengers.Where(i => i.ToFloor == null);
            if (callers.Any())
                farCaller = callers.Max(i => i.FromFloor.FloorNo);

            return farCaller;
        }

        private int GetFarPassenger(IElevator elevator)
        {
            int farPassenger = int.MinValue;
            var passengers = elevator.Passengers.Where(i => i.ToFloor != null);
            if (passengers.Any())
                farPassenger = passengers.Max(i => i.ToFloor.FloorNo);

            return farPassenger;
        }

        private int GetNearCaller(IElevator elevator)
        {
            int nearCaller = int.MaxValue;
            var callers = elevator.Passengers.Where(i => i.ToFloor == null);
            if (callers.Any())
                nearCaller = callers.Min(i => i.FromFloor.FloorNo);

            return nearCaller;
        }

        private int GetNearPassenger(IElevator elevator)
        {
            int nearPassenger = int.MaxValue;
            var passengers = elevator.Passengers.Where(i => i.ToFloor != null);
            if (passengers.Any())
                nearPassenger = passengers.Min(i => i.ToFloor.FloorNo);

            return nearPassenger;
        }

    }
}
