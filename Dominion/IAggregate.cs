namespace Dominion
{
    public interface IAggregate<out TId> : IAggregate, IEntity<TId>
    {
    }

    public interface IAggregate
    { }
}