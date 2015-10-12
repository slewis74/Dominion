using System.Linq;

namespace Realm.Repository
{
    public interface IRepository<TAggregate, in TId>
        where TAggregate : IAggregate<TId>
    {
        TAggregate Get(TId id);

        IQueryable<TAggregate> All();

        void Add(TAggregate entity);

        void Remove(TAggregate entity);
    }
}