namespace GBBassetManagementSystem.Core.Entities
{
    public abstract class EntityBase : IEntityBase
    {
        public virtual Guid Id { get; set; } = Guid.NewGuid();
    }
}