using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts;
using SmartBuilding.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding.Services.Elevator
{
    public class CallManager
    {


        //add call
        


//        List<CallOperation> callRequests = new List<CallOperation>();

//        callRequests.Add(new CallOperation(elevators, floors[33], MovementDirection.Down));//30
//    callRequests.Add(new CallOperation(elevators, floors[7], MovementDirection.Down));//4
//    callRequests.Add(new CallOperation(elevators, floors[2], MovementDirection.Down));//-1
//    callRequests.Add(new CallOperation(elevators, floors[14], MovementDirection.Down));//11


//    List<IElevator> selectedElevators = new List<IElevator>();

//        callRequests.ForEach(async callRequest =>
//    {
//        IElevator selectedElevator = await callRequest.ExecuteAsync();
//        await QueuePassengerAsync(selectedElevator, callRequest.CallerFloor, callRequest.CallerDirection);

//        if (!selectedElevators.Any(i => i.ItemId == selectedElevator.ItemId))
//            selectedElevators.Add(selectedElevator);
//    });

//    selectedElevators.ForEach(async elevator =>
//    {
//        await new MoveOperation(elevator).ExecuteAsync();
//});

    }
}
