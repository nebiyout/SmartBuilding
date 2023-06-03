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
using System.ComponentModel;

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

                    while (floors == null)
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
                    Console.Write($"Enter the maximum head count for elevator #{index} :");
                    bool elevatorLimitResult = int.TryParse(Console.ReadLine(), out elevatorLimit);

                    if (!elevatorLimitResult || elevatorLimit <= 0)
                        continue;

                    elevators.Add(new Elevator("EL-" + index, floor, elevatorLimit));
                    index++;

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
                    Console.WriteLine($" Min Floor No. ({floors[0].FloorNo})  Max Floor No. ({floors[floors.Count - 1].FloorNo})");
                    Console.WriteLine($" --------------------------------------------");
                    Console.WriteLine($"Press #1 to call elevator");
                    Console.WriteLine($"Press #2 move passangers, Pending({elevators.SelectMany(i => i.Passengers).Count(i => i.Waiting == true || (i.Waiting == false && i.ToFloor == null))})");
                    Console.WriteLine($"Press #3 to check elevator status");
                    Console.WriteLine("Press #4 to exit");
                    Console.Write("\r\nSelect an option: ");

                    switch (Console.ReadLine())
                    {
                        case "1":
                            int totalPassenger = 0;
                            bool passengerResult = false;

                            while (!passengerResult)
                            {
                                Console.Write("Enter the total number of passengers: ");
                                passengerResult = int.TryParse(Console.ReadLine(), out totalPassenger);

                                if (!passengerResult || totalPassenger <= 0)
                                    continue;

                                var callers = GetCallerData(elevators, totalPassenger, floors, out passengerResult);

                                List<IElevator> selectedElevators = new List<IElevator>();

                                callers.ForEach(call =>
                                {
                                    var selectedElevator = call.Execute();
                                    var passenger = new ElevatorPassenger(selectedElevator, call.CallerFloor, null, call.CallerDirection);
                                    var queuedElevator = new QueueOperation(selectedElevator, passenger).Execute();

                                    if (!selectedElevators.Any(i => i.ItemId == queuedElevator.ItemId))
                                        selectedElevators.Add(queuedElevator);
                                    else
                                        selectedElevators[selectedElevators.IndexOf(queuedElevator)] = queuedElevator;
                                });

                                selectedElevators.ForEach(elevator =>{ new MoveOperation(elevator).Execute(); });
                            }
                            break;

                        case "2":
                            foreach (IElevator elevator in elevators)
                            {
                                if (!elevator.Passengers.Any())
                                    continue;

                                int destinationFloorNo = int.MinValue;
                                elevator.Passengers.ForEach(passenger =>
                                {
                                    Console.WriteLine();
                                    Console.Write($"The passenger is at floor {passenger.FromFloor.FloorNo} and wants to go {passenger.Direction}. Please set destination : ");
                                    var result = int.TryParse(Console.ReadLine(), out destinationFloorNo);
                                    if (result)
                                    {
                                        var destinationFloor = floors.FirstOrDefault(i => i.FloorNo == destinationFloorNo);

                                        if (destinationFloor != null)
                                            passenger.ToFloor = destinationFloor;
                                    }
                                });

                                new MoveOperation(elevator).Execute();
                            }
                            break;

                            case "3":
                            foreach (IElevator elevator in elevators)
                            {
                                Console.WriteLine();
                                Console.WriteLine($" --------------------------------------------");
                                Console.WriteLine($"Elevator       : {elevator.ItemId}"); 
                                Console.WriteLine($"Floor No.      : {elevator.CurrentFloor.FloorNo}");
                                Console.WriteLine($"Direction      : {elevator.Direction}");
                                Console.WriteLine($"Max Head Count : {elevator.MaxPassengerLimit}");
                                Console.WriteLine($"Status         : {elevator.ItemStatus.ToString()}");
                                Console.WriteLine($"Passengers     : {elevator.Passengers.Count(i=>i.ToFloor != null)}");
                                Console.WriteLine($"Callers        : {elevator.Passengers.Count(i => i.ToFloor == null)}");
                                Console.WriteLine($" --------------------------------------------");
                            }
                            Console.ReadLine();
                            break;
                        case "4":
                        default:
                            return;
                    }
                }
            }
        }

        static List<CallOperation> GetCallerData(IEnumerable<IElevator> elevators, int totalPassenger, IList<IFloor> floors, out bool passengerResult)
        {
            passengerResult = false;
            List<CallOperation> callers = new List<CallOperation>();
            MovementDirection direction = MovementDirection.Idle;
            IFloor? callerFloor;
            int firstFloor = floors[0].FloorNo;
            int lastFloor = floors[floors.Count - 1].FloorNo;

            for (int i = 1; i <= totalPassenger; i++)
            {
                Console.WriteLine("------------------------------");
                Console.WriteLine($"Enter the Passanger #{i} data");
                Console.WriteLine("------------------------------");
                
                int floorNo = 0;

                Console.Write($"Enter your current floor number ({firstFloor} and {lastFloor}:) ");
                bool floorNoResult = int.TryParse(Console.ReadLine(), out floorNo);

                if (!floorNoResult || !(floorNo >= firstFloor && floorNo <= lastFloor))
                    break;

                callerFloor = floors.FirstOrDefault(i => i.FloorNo == floorNo);

                if (callerFloor is null)
                    break;


                Console.Write("Enter the direction (Up or Down), Press (U or D): ");
                string? inputDirection = Console.ReadLine();

                if (string.IsNullOrEmpty(inputDirection))
                    break;

                if (inputDirection.ToUpper().Equals("U") || inputDirection.ToUpper().Equals("UP"))
                    direction = MovementDirection.Up;
                else if (inputDirection.ToUpper().Equals("D") || inputDirection.ToUpper().Equals("DOWN"))
                    direction = MovementDirection.Down;
                else
                    break;
                
                callers.Add(new CallOperation(elevators, callerFloor, direction));
            }

            if(callers.Any())
            passengerResult = true;

            return callers;
        }
    }
}