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

Console.WriteLine("======================================================================");
Console.WriteLine("                   Welcome to, Smart Builing!                         ");
Console.WriteLine("======================================================================");
Console.WriteLine();

SetupBuilding();

async void SetupBuilding()
{
    Observable<ElevatorUpdateDto> observable = new Observable<ElevatorUpdateDto>();

    // Subscribe an observer
    var observer = new MovementNotification();
    var observer1 = new LoadingNotification();
    var subscription = observable.Subscribe(observer);

    IBuilding building = BuildingHelper.SetUpBuiling("Plaza Hotel");
    IBuildingProcessor buildingProcessor = BuildingHelper.GetBuildingProcessor(building);

    IList<IFloor> floors = BuildingHelper.SetupBuildingFloors(3, 35);
    buildingProcessor.AddRange<IFloor>(floors);

    buildingProcessor.Add<IElevator>(new Elevator("E01", floors[8], 7, MovementDirection.Down)); //5
    buildingProcessor.Add<IElevator>(new Elevator("E02", floors[17], 9, MovementDirection.Idle));//14
    //buildingProcessor.Add<IElevator>(new Elevator("E03", floors[38], 12, MovementDirection.Down));//35
    //buildingProcessor.Add<IElevator>(new Elevator("E04", floors[23], 4, MovementDirection.Up));//20

    IEnumerable<IElevator> elevators = buildingProcessor.GetAvailableItems<IElevator>();

    //elevators = BuildingHelper.SetupBuildingElevators(3,floors[2]);
    //buildingProcessor.AddRange<IElevator>(elevators);

    if (!elevators.Any())
        throw new NullReferenceException("No available elevator.");

    List<CallOperation> callRequests = new List<CallOperation>();

    callRequests.Add(new CallOperation(elevators, floors[33], MovementDirection.Down));//30
    callRequests.Add(new CallOperation(elevators, floors[7], MovementDirection.Down));//4
    callRequests.Add(new CallOperation(elevators, floors[2], MovementDirection.Down));//-1
    callRequests.Add(new CallOperation(elevators, floors[14], MovementDirection.Down));//11

    var groupedCalls = callRequests
        .GroupBy(i => i.CallerDirection)
        .Select(i => new
        {
            Direction = i.Key,
            Callers = i.Select(i => i).ToList()
        }).ToList();

    List<IElevator> selectedElevators = new List<IElevator>();

    groupedCalls.ForEach(groupedCall =>
    {
        groupedCall.Callers.ForEach(async call =>
        {
            IElevator selectedElevator = await call.ExecuteAsync();
            await QueuePassengerAsync(selectedElevator, call.CallerFloor, groupedCall.Direction);

            if (!selectedElevators.Any(i => i.ItemId == selectedElevator.ItemId))
                selectedElevators.Add(selectedElevator);
        });
    });

    selectedElevators.ForEach(async elevator =>
    {
        await new MoveOperation(elevator).ExecuteAsync();
    });

    Console.ReadLine();
}

async Task QueuePassengerAsync(IElevator elevator, IFloor callerFloor, MovementDirection callerMove)
{
    var passengers = new List<IElevatorPassenger>();
    var passenger = new ElevatorPassenger(elevator, callerFloor, null, callerMove);
    passengers.Add(passenger);

    var queueOperation = new QueueOperation(elevator, passengers);
    IElevator passengerQueued = await queueOperation.ExecuteAsync();
}


