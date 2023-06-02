using SmartBuilding.Contracts;
using SmartBuilding.Contracts.Floor;

namespace SmartBuilding.Core
{
    public class Floor : IFloor
    {
        public Floor(string itemId, int floorNo)
        {
            ItemId = itemId;
            FloorNo = floorNo;
        }

        public Floor(string itemId, int floorNo, IBuilding building) : this(itemId, floorNo)
        {
            Building = building;
        }

        public string ItemId { get; set; }

        public int FloorNo { get; set; }

        public IBuilding? Building { get; set; }

        public ItemStatus ItemStatus { get; set; }
    }
}
