﻿// See https://aka.ms/new-console-template for more information
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
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        LoadOperation.loadEvent += LoadOperation_loadEvent;
        SetupMenu();
    }

    private static void LoadOperation_loadEvent(IElevator elevator, IList<IFloor> floors, IElevatorPassenger passenger)
    {
        int firstFloor = floors[0].FloorNo;
        int lastFloor = floors[floors.Count - 1].FloorNo;
        int floorNo = 0;
        IFloor? elevatorFloor;
        Console.WriteLine();
        Console.WriteLine("===================================Loading Passenger=====================================");
        Console.Write($"Enter destination floor between({firstFloor} and {lastFloor}) for the passenger at floor No. #{passenger.FromFloor.FloorNo}: ");

        bool floorNoResult = int.TryParse(Console.ReadLine(), out floorNo);


        if (!floorNoResult || !(floorNo >= firstFloor && floorNo <= lastFloor))
            return;

        elevatorFloor = floors.FirstOrDefault(i => i.FloorNo == floorNo);

        if (elevatorFloor is null)
            return;

        passenger.ToFloor = floors.FirstOrDefault(i => i.FloorNo == floorNo);
    }

    private static void SetupMenu()
    {
        bool showMenu = true;

        while (showMenu)
        {
            showMenu = MainMenu();
        }
    }
    
    private static bool MainMenu()
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

                IList<IElevator> elevators = null;
                while(elevators == null)
                   elevators = CollectElevatorData(floors);

                buildingProcessor.AddRange(elevators);

                BuildDeviceOperation(buildingProcessor, floors);

                return true;

            case "2":
                return false;
            default:
                return false;
        }
    }
    
    private static IBuilding? CollectBuildingData()
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
    
    private static IList<IFloor> CollectBuildingFloorData()
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

            if (!basementResult || totalBasement < 0)
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

    private static IList<IElevator> CollectElevatorData(IList<IFloor> floors)
    {
        bool hasNoData = true;
        IList<IElevator> elevators = new List<IElevator>();

        while (hasNoData)
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
            IFloor? elevatorFloor;
            int firstFloor = floors[0].FloorNo;
            int lastFloor = floors[floors.Count - 1].FloorNo;

            for (int i = 1; i <= totalElevator; i++)
            {
                Console.WriteLine("------------------------------");
                Console.WriteLine($"Enter the elevator #{i} data");
                Console.WriteLine("------------------------------");

                int floorNo = 0;
                Console.Write($"Enter initial elevator floor No. between ({firstFloor} and {lastFloor}:) ");
                bool floorNoResult = int.TryParse(Console.ReadLine(), out floorNo);

                if (!floorNoResult || !(floorNo >= firstFloor && floorNo <= lastFloor))
                    break;

                elevatorFloor = floors.FirstOrDefault(i => i.FloorNo == floorNo);

                if (elevatorFloor is null)
                    break;

                Console.Write($"Enter elevator maximum head count :");
                bool elevatorLimitResult = int.TryParse(Console.ReadLine(), out elevatorLimit);

                if (!elevatorLimitResult || elevatorLimit <= 0)
                    continue;

                elevators.Add(new Elevator("EL-" + index, elevatorFloor, elevatorLimit));
                index++;
            }

            if (elevators.Any())
                hasNoData = false;
        }

        return elevators;
    }
    
    private static void BuildDeviceOperation(IBuildingProcessor buildingProcessor, IList<IFloor> floors)
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
                Console.WriteLine($"Press #1 to call elevator");
                Console.WriteLine($"Press #2 to move passangers, Pending({elevators.SelectMany(i => i.Passengers).Count(i => i.Waiting == true || (i.Waiting == false && i.ToFloor == null))})");
                Console.WriteLine($"Press #3 to view elevators status");
                Console.WriteLine($"Press #4 to view passengers status");
                Console.WriteLine($"Press #5 clear passenger");
                Console.WriteLine($"Press #6 to exit");
                Console.Write("\r\nSelect an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        CallElevator(elevators, floors);
                        break;

                    case "2":
                        MovePassengers(elevators, floors);
                        break;

                    case "3":
                        ShowElevatorStatus(elevators);
                        Console.WriteLine("Press any key");
                        Console.ReadLine();
                        break;

                    case "4":
                        ShowPassengerStatus(elevators);
                        Console.WriteLine("Press any key");
                        Console.ReadLine();
                        break;

                    case "5":
                        ClearPassengers(elevators);
                        Console.WriteLine("Press any key");
                        Console.ReadLine();
                        break;
                    case "6":
                    default:
                        return;
                }
            }
        }
    }
    
    private static List<CallOperation> GetCallerData(IEnumerable<IElevator> elevators, int totalPassenger, IList<IFloor> floors, out bool passengerResult)
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

            Console.Write($"Enter current floor number between ({firstFloor} and {lastFloor}): ");
            bool floorNoResult = int.TryParse(Console.ReadLine(), out floorNo);

            if (!floorNoResult || !(floorNo >= firstFloor && floorNo <= lastFloor))
                break;

            callerFloor = floors.FirstOrDefault(i => i.FloorNo == floorNo);

            if (callerFloor is null)
                break;


            Console.Write("Enter direction Press U or D :");
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

        if (callers.Any())
            passengerResult = true;

        return callers;
    }
    
    private static void CallElevator(IEnumerable<IElevator> elevators, IList<IFloor> floors)
    {
        int totalPassenger = 0;
        bool passengerResult = false;

        while (!passengerResult)
        {
            Console.Write("Enter the total number of passengers: ");
            passengerResult = int.TryParse(Console.ReadLine(), out totalPassenger);

            if (!passengerResult || totalPassenger <= 0)
                continue;

            var callers = GetCallerData(elevators, totalPassenger, floors, out passengerResult);


            callers.ForEach(call =>
            {
                var selectedElevator = call.Execute();
                var passenger = new ElevatorPassenger(selectedElevator, call.CallerFloor, null, call.CallerDirection);
                var queuedElevator = new QueueOperation(selectedElevator, passenger).Execute();
            });

            foreach (IElevator elevator in elevators)
            {
                if (!elevator.Passengers.Any())
                    continue;
                new MoveOperation(elevator).Execute();
            }
        }
    }
    
    private static void MovePassengers(IEnumerable<IElevator> elevators, IList<IFloor> floors)
    {
        foreach (IElevator elevator in elevators)
        {
            if (!elevator.Passengers.Any())
                continue;

            if (elevator.Passengers.Count(i => i.Waiting == false) > elevator.MaxPassengerLimit)
            {
                Console.WriteLine($"Elevator limit reached {elevator.MaxPassengerLimit}");
                Console.ReadLine();
                continue;
            }

            int destinationFloorNo = int.MinValue;
            elevator.Passengers.ForEach(passenger =>
            {
                Console.WriteLine();
                Console.Write($"The passenger is at {passenger.FromFloor.FloorNo} floor and wants to go {passenger.Direction}. Please set destination : ");
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
    }

    private static void ShowElevatorStatus(IEnumerable<IElevator> elevators)
    {
        foreach (IElevator elevator in elevators)
        {
            Console.WriteLine();
            Console.WriteLine($" --------------------------------------------");
            Console.WriteLine($"Elevator       : {elevator.ItemId}");
            Console.WriteLine($"Floor No.      : {elevator.CurrentFloor.FloorNo}");
            Console.WriteLine($"Direction      : {elevator.Direction}");
            Console.WriteLine($"Max Head Count : {elevator.MaxPassengerLimit}");
            Console.WriteLine($"Status         : {elevator.ItemStatus.ToString()}");
            Console.WriteLine($"Passengers     : {elevator.Passengers.Count(i => i.ToFloor != null)}");
            Console.WriteLine($"Callers        : {elevator.Passengers.Count(i => i.ToFloor == null)}");
            Console.WriteLine($" --------------------------------------------");
        }
    }
    
    private static void ClearPassengers(IEnumerable<IElevator> elevators)
    {
        foreach (IElevator elevator in elevators)
        {
            elevator.Passengers.Clear();
            elevator.ResetStatus();
        }
    }
    
    private static void ShowPassengerStatus(IEnumerable<IElevator> elevators)
    {
        foreach (IElevator elevator in elevators)
        {
            Console.WriteLine();
            Console.WriteLine($"--------------------------------------------");
            Console.WriteLine($"Elevator       : {elevator.ItemId}");
            int index = 1;

            foreach (IPassenger passenger in elevator.Passengers)
            {
                Console.WriteLine($"--------------------------------------------");
                Console.WriteLine($"Passenger #       : {index++}");
                Console.WriteLine($"Source Floor      : {passenger.FromFloor.FloorNo}");
                Console.WriteLine($"Direction         : {passenger.Direction}");
                Console.WriteLine($"Waiting           : {passenger.Waiting}");
                Console.WriteLine($"Destination Floor : {(passenger.ToFloor == null ? "-" : passenger.ToFloor.FloorNo)}");
                Console.WriteLine($"-----------------#---------------------------");
            }
        }
    }
}
