namespace SmartBuilding.Contracts
{
    public interface IOperation<T> where T : IBuildingItem
    {
        T Execute();
    }
}
