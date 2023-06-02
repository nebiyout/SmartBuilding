// See https://aka.ms/new-console-template for more information
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Contracts;
using SmartBuilding.Core;
using SmartBuilding.Services.Elevator;
using SmartBuilding.Utils;
using SmartBuilding.Utils.PubSub;
using System;

Console.WriteLine("======================================================================");
Console.WriteLine("                   Welcome to, Smart Builing!                         ");
Console.WriteLine("======================================================================");
Console.WriteLine();

SetupBuilding();

async void SetupBuilding()
{
    Observable<ElevatorMovement> observable = new Observable<ElevatorMovement>();

    // Subscribe an observer
    var observer = new MovementNotifier();
    var subscription = observable.Subscribe(observer);

    IBuilding building = BuildingHelper.SetUpBuiling("Plaza Hotel");
    IBuildingProcessor buildingProcessor = BuildingHelper.GetBuildingProcessor(building);

    IList<IFloor> floors = BuildingHelper.SetupBuildingFloors(3, 35);
    buildingProcessor.AddRange<IFloor>(floors);

    buildingProcessor.Add<IElevator>(new Elevator("E01", floors[0], 500)); //-3
    buildingProcessor.Add<IElevator>(new Elevator("E02", floors[30], 500));//27

    IEnumerable<IElevator> elevators = buildingProcessor.GetAvailableItems<IElevator>();

    //elevators = BuildingHelper.SetupBuildingElevators(3,floors[2]);
    //buildingProcessor.AddRange<IElevator>(elevators);

    if (!elevators.Any())
        throw new NullReferenceException("No available elevator.");

    var callerFloor = floors[33]; //30
    var callerMove = MoveType.Down;
    var selectedElevator = await CallElevatorAsync(elevators, callerFloor, callerMove);
    await QueuePassengerAsync(selectedElevator, callerFloor, callerMove);

    var callerFloor1 = floors[10]; //7
    var callerMove1 = MoveType.Down;
    var selectedElevator1 = await CallElevatorAsync(elevators, callerFloor1, callerMove1);
    await QueuePassengerAsync(selectedElevator1, callerFloor1, callerMove1);

    await new MoveOperation(selectedElevator, observable).ExecuteAsync();
    await new MoveOperation(selectedElevator1, observable).ExecuteAsync();

    selectedElevator.Passengers[0].Waiting = false;
    selectedElevator.Passengers[0].ToFloor = floors[3]; //0

    //selectedElevator.Passengers[1].Waiting = false;
    //selectedElevator.Passengers[1].ToFloor = floors[0];//-3

    selectedElevator1.Passengers[0].Waiting = false;
    selectedElevator1.Passengers[0].ToFloor = floors[0];//-3

    await new MoveOperation(selectedElevator, observable).ExecuteAsync();
    await new MoveOperation(selectedElevator1, observable).ExecuteAsync();

    Console.ReadLine();
}

async Task<IElevator> CallElevatorAsync(IEnumerable<IElevator> elevators, IFloor callerFloor, MoveType callerMove)
{
    var callOperation = new CallOperation(elevators, callerFloor, callerMove);
    return await callOperation.ExecuteAsync();
}

async Task QueuePassengerAsync(IElevator elevator, IFloor callerFloor, MoveType callerMove)
{
    var passengers = new List<IElevatorPassenger>();
    var passenger = new ElevatorPassenger(elevator, callerFloor, null, callerMove);
    passengers.Add(passenger);

    var queueOperation = new QueueOperation(elevator, passengers);
    IElevator passengerQueued = await queueOperation.ExecuteAsync();
}


