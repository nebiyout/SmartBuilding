using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using Moq;
using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Core;
using SmartBuilding.Services.Elevator;
using SmartBuilding.Utils;

namespace SmartBuilding.Tests
{
    public class MoveOperationTests
    {
        private IBuilding mockBuilding;
        private Mock<IElevator> elevatorMock;
        private IList<IFloor> floors;
        private IBuildingProcessor buildingProcessor;

        public MoveOperationTests()
        {
            mockBuilding = BuildingHelper.SetUpBuiling("TestBuiling");
            buildingProcessor = BuildingHelper.GetBuildingProcessor(mockBuilding);
            floors = BuildingHelper.SetupBuildingFloors(3, 10);
            buildingProcessor.AddRange<IFloor>(floors);

            elevatorMock = new Mock<IElevator>();
            elevatorMock.Setup(e => e.ItemStatus).Returns(ItemStatus.Available);
        }

        [Fact]
        public void Execute_WhenNoPassengers_ShouldResetElevatorStatus()
        {
            // Arrange
            elevatorMock.Setup(m => m.Passengers).Returns(new List<IElevatorPassenger>());

            var operation = new MoveOperation(elevatorMock.Object);

            // Act
            operation.Execute();

            // Assert
            elevatorMock.Verify(m => m.ResetStatus(), Times.Once);
        }
    }
}
