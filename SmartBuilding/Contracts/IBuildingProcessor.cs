namespace SmartBuilding.Contracts
{
    public interface IBuildingProcessor
    {
        void Add<T>(T item) where T : IBuildingItem;

        void AddRange<T>(IList<T> items) where T : IBuildingItem;

        IEnumerable<T> GetAll<T>() where T : IBuildingItem;

        IEnumerable<T> GetAvailableItems<T>() where T : IBuildingItem;

        void ClearItems();
    }
}
