using SmartBuilding.Contracts;

namespace SmartBuilding.Utils
{
    public class BuildingProcessor : IBuildingProcessor
    {
        private readonly IBuilding building;

        public BuildingProcessor(IBuilding building)
        {
            _ = building ?? throw new ArgumentNullException(nameof(building));

            this.building = building;
        }

        public void Add<T>(T item) where T : IBuildingItem
        {
            Validate(item);
            item.Building = building;
            building?.BuildingItems?.Add(item);
        }

        public void AddRange<T>(IList<T> items) where T : IBuildingItem
        {
            foreach (T t in items)
                Add(t);
        }

        public IEnumerable<T> GetAll<T>() where T : IBuildingItem
        {
            if (building.BuildingItems == null)
                throw new NullReferenceException("No building items");

            return building.BuildingItems.OfType<T>();
        }

        public IEnumerable<T> GetAvailableItems<T>() where T : IBuildingItem
        {
            if (building.BuildingItems == null)
                throw new NullReferenceException("No building items");

            return building.BuildingItems.OfType<T>().Where(i => i.ItemStatus == ItemStatus.Available);
        }

        public void ClearItems()
        {
            building?.BuildingItems?.Clear();
        }

        private void Validate<T>(T t) where T : IBuildingItem
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t));

            if (building.BuildingItems != null &&
                   building.BuildingItems.Any(i => i.ItemId == t.ItemId))
                throw new InvalidOperationException($"Existing item number - {t.ItemId}");
        }
    }
}
