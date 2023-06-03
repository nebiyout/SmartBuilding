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

                    IList<IFloor>? floors = null;

                    while (floors == null)
                        floors = CollectBuildingFloorData();
                    buildingProcessor.AddRange(floors);

                    var itemType = ItemTypeSelector();

                    bool result  =  BuildItemType(itemType,buildingProcessor, floors[0]);
      
                    if(result)
                        BuildDeviceOperation(buildingProcessor, floors, itemType);

                    return result;

                case "2":
                    return false;
                default:
                    return false;
            }
        }

        static IBuilding CollectBuildingData()
        {
            Console.Clear();
            Console.WriteLine("===========================================================");
            Console.WriteLine("  Enter the building Information and press anykey   ");
            Console.WriteLine("===========================================================");
            Console.WriteLine();
            Console.Write("Enter Building Name: ");
            string name = Console.ReadLine();

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
                Console.Write("Total No. of basement: ");
                bool basementResult = int.TryParse(Console.ReadLine(), out totalBasement);

                if (!basementResult || totalBasement <= 0)
                    continue;

                Console.Write("Total No. of floors: ");
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
                Console.Write("Total No. of elevators: ");
                bool elevatorResult = int.TryParse(Console.ReadLine(), out totalElevator);


                if (!elevatorResult || totalElevator <= 0)
                    continue;

                int elevatorLimit = 0;
                int index = 1;

                do
                {
                    Console.Write($"Enter the maximum person limit for elevator {index} :");
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

        static ItemType ItemTypeSelector()
        {
            Console.Clear();
            Console.WriteLine("==============================================");
            Console.WriteLine($"               Select Device                 ");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.Write("\r\nSelect an option: \n");
            int index = 1;
            foreach (var itemType in Enum.GetNames(typeof(ItemType)))
            {
                Console.WriteLine($"Press #{index} for {itemType}");
                index++;    
            }

            switch (Console.ReadLine())
            {
                case "1":
                    return (ItemType)Enum.Parse(typeof(ItemType), "0");

                case "2":
                    return (ItemType)Enum.Parse(typeof(ItemType), "1");

                case "3":
                    return (ItemType)Enum.Parse(typeof(ItemType), "2");

                default:
                    return ItemType.Elevator;

            }
        }

        static bool BuildItemType(ItemType itemType, IBuildingProcessor buildingProcessor, IFloor floor)
        {
            switch (itemType)
            {
                case ItemType.Elevator:
                    var elevators = CollectElevatorData(floor);
                    buildingProcessor.AddRange(elevators);
                    return true;

                default:
                    return false;
            }
        }

        static void BuildDeviceOperation(IBuildingProcessor buildingProcessor, IList<IFloor>? floors, ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Elevator:

                    IEnumerable<IElevator> elevators = buildingProcessor.GetAvailableItems<IElevator>();

                    if (elevators.Any() && floors.Any())
                    {
                        Console.Clear();
                        Console.WriteLine("==============================================");
                        Console.WriteLine($"         Select Elevator Operation           ");
                        Console.WriteLine("==============================================");
                        Console.WriteLine("Press #1 to call elevator");
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
                            default:
                                break;
                        }
                    }

                    //List<CallOperation> callRequests = new List<CallOperation>();
                    //callRequests.Add(new CallOperation(elevators, floors[33], MovementDirection.Down));//30
                    //callRequests.Add(new CallOperation(elevators, floors[7], MovementDirection.Down));//4
                    //callRequests.Add(new CallOperation(elevators, floors[2], MovementDirection.Down));//-1
                    //callRequests.Add(new CallOperation(elevators, floors[14], MovementDirection.Down));//11

                    //List<IElevator> selectedElevators = new List<IElevator>();

                    //callRequests.ForEach(callRequest =>
                    //{
                    //    IElevator selectedElevator = callRequest.Execute();
                    //    QueuePassenger(selectedElevator, callRequest.CallerFloor, callRequest.CallerDirection);

                    //    if (!selectedElevators.Any(i => i.ItemId == selectedElevator.ItemId))
                    //        selectedElevators.Add(selectedElevator);
                    //});

                    //selectedElevators.ForEach(elevator =>
                    //{
                    //    new MoveOperation(elevator).Execute();
                    //});

                    break;
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
                Console.WriteLine("   Enter the calling data and press anykey   ");
                Console.WriteLine("===========================================================");
                Console.WriteLine();
                Console.Write("\r\nSelect an option: ");
                Console.WriteLine();
                Console.Write($"Enter your current Floor No.: between {firstFloor} and {lastFloor}: ");
                bool floorNoResult = int.TryParse(Console.ReadLine(), out floorNo);

                if (!floorNoResult || !(floorNo >= firstFloor && floorNo <= lastFloor))
                    continue;

                callerFloor = floors.FirstOrDefault(i => i.FloorNo == floorNo);

                if (callerFloor is null)
                    continue;

                Console.Write("Enter the total No. of passengers: ");
                bool passengerResult = int.TryParse(Console.ReadLine(), out totalPassenger);

                if (!passengerResult || totalPassenger <= 0)
                    continue;

                Console.Write("Enter the Direction (Up or Down): Press (U or D): ");
                string inputDirection = Console.ReadLine();

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

        static void QueuePassenger(IElevator elevator, IFloor callerFloor, MovementDirection callerMove)
        {
            var passengers = new List<IElevatorPassenger>();
            var passenger = new ElevatorPassenger(elevator, callerFloor, null, callerMove);
            passengers.Add(passenger);

            var queueOperation = new QueueOperation(elevator, passengers).Execute();
        }

       
        //Observable<ElevatorUpdateDto> observable = new Observable<ElevatorUpdateDto>();

        //IBuilding building = BuildingHelper.SetUpBuiling("Plaza Hotel");
        //IBuildingProcessor buildingProcessor = BuildingHelper.GetBuildingProcessor(building);

        //IList<IFloor> floors = BuildingHelper.SetupBuildingFloors(3, 35);
        //buildingProcessor.AddRange<IFloor>(floors);

        //buildingProcessor.Add<IElevator>(new Elevator("E01", floors[8], 7, MovementDirection.Down)); //5
        ////buildingProcessor.Add<IElevator>(new Elevator("E02", floors[17], 9, MovementDirection.Idle));//14
        ////buildingProcessor.Add<IElevator>(new Elevator("E03", floors[38], 12, MovementDirection.Down));//35
        ////buildingProcessor.Add<IElevator>(new Elevator("E04", floors[23], 4, MovementDirection.Up));//20

        //IEnumerable<IElevator> elevators = buildingProcessor.GetAvailableItems<IElevator>();

        //if (!elevators.Any())
        //    throw new NullReferenceException("No available elevator.");

        //List<CallOperation> callRequests = new List<CallOperation>();

        //callRequests.Add(new CallOperation(elevators, floors[33], MovementDirection.Down));//30
        //callRequests.Add(new CallOperation(elevators, floors[7], MovementDirection.Down));//4
        //callRequests.Add(new CallOperation(elevators, floors[2], MovementDirection.Down));//-1
        //callRequests.Add(new CallOperation(elevators, floors[14], MovementDirection.Down));//11

        //List<IElevator> selectedElevators = new List<IElevator>();

        //callRequests.ForEach(callRequest =>
        //{
        //    IElevator selectedElevator = callRequest.Execute();
        //    QueuePassenger(selectedElevator, callRequest.CallerFloor, callRequest.CallerDirection);

        //    if (!selectedElevators.Any(i => i.ItemId == selectedElevator.ItemId))
        //        selectedElevators.Add(selectedElevator);
        //});

        //selectedElevators.ForEach(elevator =>
        //{
        //    new MoveOperation(elevator).Execute();
        //});

        //Console.ReadLine();


    }

}