using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Queries.Core.Repositories
{
    // TEntity ---> works for any entity type (generic); where TEntity : class ---> restriction which indicates that TEntity must be a class type
    // The main idea is that IRepository could be used to handle Author, Course, Cover, etc.
    public interface IRepository<TEntity> where TEntity : class
    {
        // finding objects methods
        TEntity Get(int id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        // adding objects methods
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);

        // removing objects methods
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
