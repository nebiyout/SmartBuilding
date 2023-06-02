namespace SmartBuilding.Contracts
{
    public interface IOperation<T> where T : IBuildingItem
    {
        Task<T> ExecuteAsync();
    }
}
