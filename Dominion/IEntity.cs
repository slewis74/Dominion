namespace Dominion
{
    public interface IEntity<out TId>
    {
        TId Id { get; }
    }
}