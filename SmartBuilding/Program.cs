// See https://aka.ms/new-console-template for more information
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Contracts;
using SmartBuilding.Core;
using SmartBuilding.Services.Elevator;
using SmartBuilding.Utils;
using SmartBuilding.Utils.PubSub;
using System;
using SmartBuilding.Core.Dto;
using SmartBuilding.Services.Elevator.Notification;
using System.Runtime.Serialization;
using System.Reflection.Metadata.Ecma335;
using System.Reflection;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {

        SetupMenu();

        void SetupMenu()
        {
            bool showMenu = true;

            while (showMenu)
            {
                showMenu = MainMenu();
            }
        }

        static bool MainMenu()
        {
            IBuilding? building;

            Console.Clear();
            Console.WriteLine("======================================================================");
            Console.WriteLine($"                   Welcome to Smart Builing!                         ");
            Console.WriteLine("======================================================================");
            Console.WriteLine();
            Console.WriteLine("Press #1 to create a building");
            Console.WriteLine("Press #2 to exit");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    building = CollectBuildingData();
                    if (building is null)
                        return true;
                    
                    IBuildingProcessor buildingProcessor = BuildingHelper.GetBuildingProcessor(building);
                    buildingProcessor.ClearItems();

                    IList<IFloor>? floors = null;

                    while(floors == null)
                    floors = CollectBuildingFloorData();
                    buildingProcessor.AddRange(floors);

                    IList<IElevator> elevators = CollectElevatorData(floors[0]);
                    buildingProcessor.AddRange(elevators);

                    BuildDeviceOperation(buildingProcessor, floors);

                    return true;

                case "2":
                    return false;
                default:
                    return false;
            }
        }

        static IBuilding? CollectBuildingData()
        {
            Console.Clear();
            Console.WriteLine("===========================================================");
            Console.WriteLine("  Enter the building information and press anykey   ");
            Console.WriteLine("===========================================================");
            Console.WriteLine();
            Console.Write("Enter the Building Name: ");
            string? name = Console.ReadLine();

            if (string.IsNullOrEmpty(name))
                return null;

            return BuildingHelper.SetUpBuiling(name.Trim());
        }

        static IList<IFloor> CollectBuildingFloorData()
        {
            bool collectData = true;

            while (collectData)
            {
                int totalBasement = 0;
                int totalFloor = 0;
                Console.Clear();
                Console.WriteLine("===========================================================");
                Console.WriteLine("   Enter the floor data and press anykey   ");
                Console.WriteLine("===========================================================");
                Console.WriteLine();
                Console.Write("Enter total number of basement: ");
                bool basementResult = int.TryParse(Console.ReadLine(), out totalBasement);

                if (!basementResult || totalBasement <= 0)
                    continue;

                Console.Write("Enter total number of floor: ");
                bool floorResult = int.TryParse(Console.ReadLine(), out totalFloor);


                if (!floorResult || totalFloor <= 0)
                    continue;

                collectData = false;

                return BuildingHelper.SetupBuildingFloors(totalBasement, totalFloor);
            }

            return null;
        }

        static IList<IElevator> CollectElevatorData(IFloor floor)
        {
            bool collectData = true;
            IList<IElevator> elevators = new List<IElevator>();

            while (collectData)
            {
                int totalElevator = 0;
                Console.Clear();
                Console.WriteLine("===========================================================");
                Console.WriteLine($"   Enter the elevator data and press anykey   ");
                Console.WriteLine("===========================================================");
                Console.WriteLine();                
                Console.Write("Enter the total number of elevator: ");
                bool elevatorResult = int.TryParse(Console.ReadLine(), out totalElevator);


                if (!elevatorResult || totalElevator <= 0)
                    continue;

                int elevatorLimit = 0;
                int index = 1;

                do
                {
                    Console.Write($"Enter the maximum head count limit #{index} :");
                    bool elevatorLimitResult = int.TryParse(Console.ReadLine(), out elevatorLimit);

                    if (!elevatorLimitResult || elevatorLimit <= 0)
                        continue;

                    index++;

                    elevators.Add(new Elevator("EL-" + index, floor, elevatorLimit));
                } while (index <= totalElevator);
                collectData = false;
            }

            return elevators;
        }

        static void BuildDeviceOperation(IBuildingProcessor buildingProcessor, IList<IFloor> floors)
        {
            IEnumerable<IElevator> elevators = buildingProcessor.GetAvailableItems<IElevator>();

            if (elevators.Any() && floors.Any())
            {
                while (true)
                {

                    Console.Clear();
                    Console.WriteLine("=============================================");
                    Console.WriteLine($"         Select Elevator Operation          ");
                    Console.WriteLine("============================================="); 
                    Console.WriteLine($" Minimum Floor No. ({floors[0].FloorNo})");
                    Console.WriteLine($" Maximum Floor No. ({floors[floors.Count-1].FloorNo})");
                    Console.WriteLine($" ------------------------------------------");
                    Console.WriteLine("Press #1 to call elevator");
                    Console.WriteLine($"Press #2 to go to your destination Pending({elevators.SelectMany(i=>i.Passengers).Count(i=>i.Waiting == true || (i.Waiting == false && i.ToFloor == null)) })" );
                    Console.WriteLine("Press #3 to exit");
                    Console.Write("\r\nSelect an option: ");

                    switch (Console.ReadLine())
                    {
                        case "1":
                            IFloor callerFloor;
                            MovementDirection direction;
                            int noOfPassengers = 0;
                            var passengers = new List<IElevatorPassenger>();

                            CallOperation caller = CollectElevatorCallData(elevators, floors,
                                out callerFloor, out direction, out noOfPassengers);
                            var selectedElevator = caller.Execute();

                            if (callerFloor != null && noOfPassengers > 0)
                            {
                                for (int i = 1; i <= noOfPassengers; i++)
                                {
                                    var passenger = new ElevatorPassenger(selectedElevator, callerFloor, null, direction);
                                    passengers.Add(passenger);
                                }

                                var queueOperation = new QueueOperation(selectedElevator, passengers).Execute();
                                new MoveOperation(selectedElevator).Execute();
                            }

                            break;

                        case "2":
                            foreach (IElevator elevator in elevators)
                            {
                                if (!elevator.Passengers.Any())
                                    continue;

                                int index = 1;
                                int destinationFloorNo = int.MinValue;
                                elevator.Passengers.ForEach(passenger =>
                                {
                                    Console.WriteLine();
                                    Console.Write($"Set the destination for passenger number #{index} : ");
                                    var result = int.TryParse(Console.ReadLine(), out destinationFloorNo);
                                    if (result)
                                    {
                                        var destinationFloor = floors.FirstOrDefault(i => i.FloorNo == destinationFloorNo);

                                        if (destinationFloor != null)
                                            passenger.ToFloor = destinationFloor;

                                        index++;
                                    }
                                });

                                new MoveOperation(elevator).Execute();
                            }
                            break;

                        case "3":
                        default:
                            return;
                    }
                }
            }
        }
        static CallOperation CollectElevatorCallData(IEnumerable<IElevator> elevators, 
        IList<IFloor> floors, out IFloor? callerFloor,
        out MovementDirection direction, out int totalPassenger)
        {
            int firstFloor = floors[0].FloorNo;
            int lastFloor = floors[floors.Count - 1].FloorNo;
            callerFloor = null;
            direction = MovementDirection.Idle;
            totalPassenger = 0;
            bool collectData = true;

            while (collectData)
            {
                int floorNo = 0;
                Console.Clear();
                Console.WriteLine("===========================================================");
                Console.WriteLine("   Enter the caller data and press anykey   ");
                Console.WriteLine("===========================================================");
                Console.WriteLine();
                Console.Write("\r\nSelect an option: ");
                Console.WriteLine();
                Console.Write($"Enter your floor number between {firstFloor} and {lastFloor}: ");
                bool floorNoResult = int.TryParse(Console.ReadLine(), out floorNo);

                if (!floorNoResult || !(floorNo >= firstFloor && floorNo <= lastFloor))
                    continue;

                callerFloor = floors.FirstOrDefault(i => i.FloorNo == floorNo);

                if (callerFloor is null)
                    continue;

                Console.Write("Enter the total number of passengers: ");
                bool passengerResult = int.TryParse(Console.ReadLine(), out totalPassenger);

                if (!passengerResult || totalPassenger <= 0)
                    continue;

                Console.Write("Enter the Direction (Up or Down): Press (U or D): ");
                string? inputDirection = Console.ReadLine();

                if (string.IsNullOrEmpty(inputDirection))
                    continue;

                if (inputDirection.ToUpper().Equals("U"))
                    direction = MovementDirection.Up;
                else if (inputDirection.ToUpper().Equals("D"))
                    direction = MovementDirection.Down;
                else
                    continue;
                
                collectData = false;

                return new CallOperation(elevators, callerFloor, direction);
            }

            return null;
        }
    }
}