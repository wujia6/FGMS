using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Repositories.Interfaces;
using FGMS.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace FGMS.Repositories.Implements
{
    internal class BaseRepository<T> : IBaseRepository<T> where T : class, new()
    {
        private readonly IFgmsDbRepository<T> repository;

        public BaseRepository(IFgmsDbRepository<T> repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public bool AddEntity(IEnumerable<T> entities)
        {
            return repository.Insert(entities);
        }

        public bool AddEntity(T entity)
        {
            return repository.Insert(entity);
        }

        public bool DeleteEntity(IEnumerable<T> entities)
        {
            return repository.Delete(entities);
        }

        public bool DeleteEntity(T entity)
        {
            return repository.Delete(entity);
        }

        public bool UpdateEntity(IEnumerable<T> entities, Expression<Func<T, object>>[]? fields = null)
        {
            return repository.Update(entities, fields);
        }

        public bool UpdateEntity(T entity, Expression<Func<T, object>>[]? fields = null)
        {
            return repository.Update(entity, fields);
        }

        public async Task<T> GetEntityAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = repository.Entities;
            if (include != null)
                query = include(query);
            return await query.FirstOrDefaultAsync(expression);
        }

        public async Task<IEnumerable<T>> GetListAsync(Expression<Func<T, bool>>? expression = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = repository.Entities;  // 获取初始 IQueryable，不修改原始属性
            if (include != null)
                query = include(query);
            if (expression != null)
                query = query.Where(expression);
            return await query.ToListAsync();
        }

        public IQueryable<T> GetQueryable(Expression<Func<T, bool>>? expression = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            //IQueryable<T> query = repository.Entities;  // 获取初始 IQueryable，不修改原始属性
            //if (include != null)
            //    query = include(query);
            //if (expression != null)
            //    query = query.Where(expression);
            //return query;
            return new QueryableBuilder<T>(repository.Entities).Where(expression).Include(include).Build();
        }
    }
}
