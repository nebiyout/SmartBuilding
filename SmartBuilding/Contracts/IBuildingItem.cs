namespace SmartBuilding.Contracts
{
    public interface IBuildingItem
    {
        string ItemId { get; set; }

        ItemStatus ItemStatus { get; set; }

        IBuilding? Building { get; set; }
    }
}