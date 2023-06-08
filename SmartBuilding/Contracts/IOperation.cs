using System.Threading.Tasks;

namespace SmartBuilding.Contracts
{
    public interface IOperation<T> where T : IBuildingItem
    {
        T Execute();
    }

    public interface IOperationAsync<T> where T : IBuildingItem
    {
        Task<T> ExecuteAsync();
    }
}
