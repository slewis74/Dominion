namespace Dominion
{
    public abstract class Entity<TId> : IEntity<TId>
    {
        protected Entity()
        {
        }

        protected Entity(TId id)
        {
            Id = id;
        }

        public TId Id { get; private set; }
    }
}