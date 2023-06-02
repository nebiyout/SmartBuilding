using SmartBuilding.Contracts;

namespace SmartBuilding.Core
{
    public class Building : IBuilding
    {
        public Building(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
            BuildingItems = new List<IBuildingItem>();
        }

        public string Name { get; set; }

        public IList<IBuildingItem>? BuildingItems { get; set; }
    }
}
