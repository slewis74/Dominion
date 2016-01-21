using System.Linq;

namespace Dominion.Repositories
{
    /// <summary>
    /// Marker interface to make container registration easier.
    /// </summary>
    public interface IRepository
    {
    }

    public interface IRepository<TAggregate, in TId> : IRepository
        where TAggregate : IAggregate<TId>
    {
        TAggregate Get(TId id);

        IQueryable<TAggregate> All();

        void Add(TAggregate entity);

        void Remove(TAggregate entity);
    }
}