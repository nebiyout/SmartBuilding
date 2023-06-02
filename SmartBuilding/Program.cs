// See https://aka.ms/new-console-template for more information
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Contracts;
using SmartBuilding.Core;
using SmartBuilding.Services.Elevator;
using SmartBuilding.Utils;

Console.WriteLine("Welcome to, Smart Builing!");

SetupBuilding();

async void SetupBuilding()
{
    IBuilding building = BuildingHelper.SetUpBuiling("Plaza Hotel");
    IBuildingProcessor buildingProcessor = BuildingHelper.GetBuildingProcessor(building);

    IList<IFloor> floors = BuildingHelper.SetupBuildingFloors(3, 20);
    buildingProcessor.AddRange<IFloor>(floors);

    buildingProcessor.Add<IElevator>(new Elevator("E01", floors[0], 500));
    buildingProcessor.Add<IElevator>(new Elevator("E02", floors[14], 500));

    IEnumerable<IElevator> elevators = buildingProcessor.GetAvailableItems<IElevator>();

    //elevators = BuildingHelper.SetupBuildingElevators(3,floors[2]);
    //buildingProcessor.AddRange<IElevator>(elevators);

    if (!elevators.Any())
        throw new NullReferenceException("No available elevator.");

    var callerFloor = floors[18]; //15
    var callerMove = MoveType.Down;
    var selectedElevator = await CallElevatorAsync(elevators, callerFloor, callerMove);
    await QueuePassengerAsync(selectedElevator, callerFloor, callerMove);


    var callerFloor1 = floors[7]; //4
    var callerMove1 = MoveType.Down;
    var selectedElevator1 = await CallElevatorAsync(elevators, callerFloor1, callerMove1);
    await QueuePassengerAsync(selectedElevator1, callerFloor1, callerMove1);
    var moveElevator = new MoveOperation(selectedElevator).ExecuteAsync();
    var moveElevator1 = new MoveOperation(selectedElevator1).ExecuteAsync();

    selectedElevator.Passengers[0].Waiting = false;
    selectedElevator.Passengers[0].ToFloor = floors[3]; //0
    await new MoveOperation(selectedElevator).ExecuteAsync();

    selectedElevator1.Passengers[0].Waiting = false;
    selectedElevator1.Passengers[0].ToFloor = floors[0];//-3
    await new MoveOperation(selectedElevator1).ExecuteAsync();




    Console.ReadLine();
}

async Task<IElevator> CallElevatorAsync(IEnumerable<IElevator> elevators, IFloor callerFloor, MoveType callerMove)
{
    var callOperation = new CallOperation<IElevator>(elevators, callerFloor, callerMove);
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


