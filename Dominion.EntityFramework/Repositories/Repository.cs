using System.Data.Entity;
using System.Linq;
using Dominion.Repositories;

namespace Dominion.EntityFramework.Repositories
{
    public abstract class Repository<TAggregate, TId> : IRepository<TAggregate, TId>
        where TAggregate : class, IAggregate<TId>
    {
        private readonly DbContext _context;

        protected Repository(DbContext context)
        {
            _context = context;
        }

        public TAggregate Get(TId id)
        {
            return _context.Set<TAggregate>().Find(id);
        }

        public IQueryable<TAggregate> All()
        {
            return _context.Set<TAggregate>();
        }

        public void Add(TAggregate entity)
        {
            _context.Set<TAggregate>().Add(entity);
        }

        public void Remove(TAggregate entity)
        {
            _context.Set<TAggregate>().Remove(entity);
        }
    }
}