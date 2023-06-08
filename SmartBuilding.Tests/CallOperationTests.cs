using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using Moq;
using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Services.Elevator;

namespace SmartBuilding.Tests
{
    public class CallOperationTests
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenElevatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CallOperation(null, new Mock<IFloor>().Object, MovementDirection.Up));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenFloorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CallOperation(new List<IElevator> { new Mock<IElevator>().Object }, null, MovementDirection.Up));
        }

        [Fact]
        public void Execute_ThrowsArgumentException_WhenNoElevatorFound()
        {
            Assert.Throws<ArgumentException>(() => new CallOperation(new List<IElevator>(), new Mock<IFloor>().Object, MovementDirection.Up));
        }

        [Fact]
        public async Task Execute_ThrowsArgumentException_WhenNoAvailableElevatorFound()
        {
            //arrange
            var elevatorMock = new Mock<IElevator>();
            elevatorMock.Setup(e => e.ItemStatus).Returns(ItemStatus.OutOfService);
            var elevators = new List<IElevator> { elevatorMock.Object }; // no available elevators found
            var operation = new CallOperation(elevators, new Mock<IFloor>().Object, MovementDirection.Up);

            //act
            await Assert.ThrowsAsync<ArgumentException>(async () => await operation.ExecuteAsync());
        }

        [Fact]
        public async Task Execute_ReturnsClosestElevator_WhenElevatorAtSameFloor()
        {
            // arrange
            var elevatorMock = new Mock<IElevator>();
            elevatorMock.Setup(e => e.CurrentFloor.FloorNo).Returns(1);

            var floorMock = new Mock<IFloor>();
            floorMock.Setup(f => f.FloorNo).Returns(1);

            var operation = new CallOperation(new List<IElevator> { elevatorMock.Object }, floorMock.Object, MovementDirection.Up);

            // act
            var result = await operation.ExecuteAsync();

            // assert
            Assert.Equal(elevatorMock.Object, result);
        }

        [Fact]
        public async Task Execute_ReturnsFirstElevator_WhenMultipleElevatorsAtSameFloor()
        {
            // arrange
            var elevatorMock1 = new Mock<IElevator>();
            elevatorMock1.Setup(e => e.CurrentFloor.FloorNo).Returns(1);
            elevatorMock1.Setup(e => e.Passengers).Returns(new List<IElevatorPassenger>());


            var elevatorMock2 = new Mock<IElevator>();
            elevatorMock2.Setup(e => e.CurrentFloor.FloorNo).Returns(1);
            elevatorMock1.Setup(e => e.Passengers).Returns(new List<IElevatorPassenger>());


            var callerFloorMock = new Mock<IFloor>();
            callerFloorMock.Setup(f => f.FloorNo).Returns(1);

            var operation = new CallOperation(new List<IElevator> { elevatorMock1.Object, elevatorMock2.Object }, callerFloorMock.Object, MovementDirection.Up);

            // act
            var result = await operation.ExecuteAsync();

            // assert
            Assert.True(result == elevatorMock1.Object); // any of the elevators at the same floor can be returned
        }

        [Fact]
        public async Task Execute_ReturnsClosestElevator_WhenClosestElevatorIdle()
        {
            // arrange
            //idle elevator
            var elevatorMock1 = new Mock<IElevator>();
            elevatorMock1.Setup(e => e.CurrentFloor.FloorNo).Returns(1);
            elevatorMock1.Setup(e => e.Direction).Returns(MovementDirection.Idle);
            elevatorMock1.Setup(e => e.Passengers).Returns(new List<IElevatorPassenger>());

            //moving elevator
            var elevatorMock2 = new Mock<IElevator>();
            elevatorMock2.Setup(e => e.CurrentFloor.FloorNo).Returns(5);
            elevatorMock2.Setup(e => e.Direction).Returns(MovementDirection.Up);
            elevatorMock2.Setup(e => e.Passengers).Returns(new List<IElevatorPassenger>());

            var callerFloorMock = new Mock<IFloor>();
            callerFloorMock.Setup(f => f.FloorNo).Returns(2);

            var operation = new CallOperation(new List<IElevator> { elevatorMock1.Object, elevatorMock2.Object }, callerFloorMock.Object, MovementDirection.Up);

            // act
            var result = await operation.ExecuteAsync();

            // assert
            Assert.Equal(elevatorMock1.Object, result); // expect the idle elevator to be selected as it's closer
        }


        [Fact]
        public async Task Execute_ReturnsClosestElevator_WhenClosestElevatorMovingSameDirectionToCaller()
        {
            // arrange
            //idle elevator
            var elevatorMock1 = new Mock<IElevator>();
            elevatorMock1.Setup(e => e.CurrentFloor.FloorNo).Returns(1);
            elevatorMock1.Setup(e => e.Direction).Returns(MovementDirection.Idle);
            elevatorMock1.Setup(e => e.Passengers).Returns(new List<IElevatorPassenger>());

            //moving elevator
            var elevatorMock2 = new Mock<IElevator>();
            elevatorMock2.Setup(e => e.CurrentFloor.FloorNo).Returns(5);
            elevatorMock2.Setup(e => e.Direction).Returns(MovementDirection.Up);
            elevatorMock2.Setup(e => e.Passengers).Returns(new List<IElevatorPassenger>());

            var callerFloorMock = new Mock<IFloor>();
            callerFloorMock.Setup(f => f.FloorNo).Returns(4);

            var operation = new CallOperation(new List<IElevator> { elevatorMock1.Object, elevatorMock2.Object }, callerFloorMock.Object, MovementDirection.Up);

            // act
            var result = await operation.ExecuteAsync();

            // assert
            Assert.Equal(elevatorMock2.Object, result); // expect the moving elevator to be selected as it's closer
        }


        [Fact]
        public async Task Execute_ReturnsClosestElevator_WhenClosestElevatorUpAndCallerUpButElevatorHigher()
        {
            // arrange
            var elevatorMock = new Mock<IElevator>();
            elevatorMock.Setup(e => e.CurrentFloor.FloorNo).Returns(3);
            elevatorMock.Setup(e => e.Direction).Returns(MovementDirection.Up);
            elevatorMock.Setup(e => e.Passengers).Returns(new List<IElevatorPassenger>());

            var callerFloorMock = new Mock<IFloor>();
            callerFloorMock.Setup(f => f.FloorNo).Returns(2);

            var operation = new CallOperation(new List<IElevator> { elevatorMock.Object }, callerFloorMock.Object, MovementDirection.Up);

            // act
            var result = await operation.ExecuteAsync();

            // assert
            Assert.Equal(elevatorMock.Object, result); // even though the elevator is going up and it's higher, it's still the closest elevator
        }

        // Test Case for when the closest elevator is going down, the caller is also going down, but the elevator is at a lower floor
        [Fact]
        public async Task Execute_ReturnsClosestElevator_WhenClosestElevatorDownAndCallerDownButElevatorLower()
        {
            // arrange
            var elevatorMock = new Mock<IElevator>();
            elevatorMock.Setup(e => e.CurrentFloor.FloorNo).Returns(1);
            elevatorMock.Setup(e => e.Direction).Returns(MovementDirection.Down);
            elevatorMock.Setup(e => e.Passengers).Returns(new List<IElevatorPassenger>());

            var callerFloorMock = new Mock<IFloor>();
            callerFloorMock.Setup(f => f.FloorNo).Returns(2);

            var operation = new CallOperation(new List<IElevator> { elevatorMock.Object }, callerFloorMock.Object, MovementDirection.Down);

            // act
            var result = await operation.ExecuteAsync();

            // assert
            Assert.Equal(elevatorMock.Object, result); // even though the elevator is going down and it's lower, it's still the closest elevator
        }

        // Test Case for when the closest elevator is going down, but the caller is going up
        [Fact]
        public async Task Execute_ReturnsClosestElevator_WhenClosestElevatorDownAndCallerUp()
        {
            // arrange
            var elevatorMock = new Mock<IElevator>();
            elevatorMock.Setup(e => e.CurrentFloor.FloorNo).Returns(1);
            elevatorMock.Setup(e => e.Direction).Returns(MovementDirection.Down);
            elevatorMock.Setup(e => e.Passengers).Returns(new List<IElevatorPassenger>());

            var callerFloorMock = new Mock<IFloor>();
            callerFloorMock.Setup(f => f.FloorNo).Returns(2);

            var operation = new CallOperation(new List<IElevator> { elevatorMock.Object }, callerFloorMock.Object, MovementDirection.Up);

            // act
            var result = await operation.ExecuteAsync();

            // assert
            Assert.Equal(elevatorMock.Object, result); // even though the elevator is going down and the caller is going up, it's still the closest elevator
        }
    }   
}