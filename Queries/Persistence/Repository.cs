using Queries.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Queries.Persistence
{
    // GENERIC implementation of IRepository interface
    public class Repository<TEntity>: IRepository<TEntity> where TEntity : class
    {
        // DBContext which is injected into the repository
        protected readonly DbContext Context; // it's protected because in derived classes we might want to use it

        // Constructor that initializes the Context property
        public Repository(DbContext context) // generics DBContext, nothing to do with our specific application
        {
            Context = context;
        }

        // Method to get an entity by its ID
        public TEntity Get(int id)
        {
            return Context.Set<TEntity>().Find(id); // Set<TEntity>() ---> gets the DbSet for the specific entity type TEntity and then executes Find(id) against that DbSet
        }

        // Method to get all entities of type TEntity
        public IEnumerable<TEntity> GetAll()
        {
            return Context.Set<TEntity>().ToList();
        }

        // Method to find a collection of entities based on a predicate
        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().Where(predicate);
        }

        // Method to add an entity
        public void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        // Method to add a range of entities
        public void AddRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().AddRange(entities);
        }

        // Method to remove an entity
        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        // Method to remove a range of entities
        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
        }
    }
}
