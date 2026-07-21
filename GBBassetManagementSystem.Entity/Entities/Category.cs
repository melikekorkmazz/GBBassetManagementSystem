using GBBassetManagementSystem.Core.Entities;

namespace GBBassetManagementSystem.Entity.Entities
{
    public class Category : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}