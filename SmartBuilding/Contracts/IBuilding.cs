namespace SmartBuilding.Contracts
{
    public interface IBuilding
    {
        string Name { get; set; }

        IList<IBuildingItem>? BuildingItems { get; set; }
    }
}
