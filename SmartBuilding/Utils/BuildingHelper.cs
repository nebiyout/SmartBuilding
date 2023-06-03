using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Elevator;
using SmartBuilding.Contracts.Floor;
using SmartBuilding.Core;
using SmartBuilding.Services.Display;
using System.IO.Pipes;

namespace SmartBuilding.Utils
{
    public static class BuildingHelper
    {
        private static IBuildingProcessor? buildingProcessor;

        public static IBuilding SetUpBuiling(string buildName)
        {
            IBuilding building = new Building("Plaza Hotel");
            return building;
        }

        public static IBuildingProcessor GetBuildingProcessor(IBuilding building)
        {
            if (buildingProcessor == null)
                buildingProcessor = new BuildingProcessor(building);

            return buildingProcessor;
        }

        public static IList<IFloor> SetupBuildingFloors(int totalBasement, int totalFloor)
        {
            var floors = new List<IFloor>();

            //basement
            for (int i = -1 * totalBasement; i < 0; i++)
                floors.Add(new Floor("bf" + i, i));

            //ground floor
            floors.Add(new Floor("bf0", 0));

            //upper floors
            for (int i = 1; i <= totalFloor; i++)
                floors.Add(new Floor("bf" + i, i));

            return floors;
        }

        public static T CreateInstance<T>() where T : IBuildingItem
        {
            return Activator.CreateInstance<T>();
        }


        public static IList<IElevator> SetupBuildingElevators(int noOfElevators, IFloor startingFloor)
        {
            var elevators = new List<IElevator>();

            for (int i = 1; i <= noOfElevators; i++)
                elevators.Add(new Elevator("El-" + i, startingFloor, 500));

            return elevators;
        }

        public static IEnumerable<T> GetItems<T>() where T: IBuildingItem
        {
            _= buildingProcessor ?? throw new ArgumentNullException(nameof(buildingProcessor));

            var items = buildingProcessor.GetAvailableItems<T>();

            return items;
        }
    }
}
