using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace FGMS.Repositories.Interfaces
{
    public interface IBaseRepository<T> where T : class, new()
    {
        Task<T> GetEntityAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

        Task<IEnumerable<T>> GetListAsync(Expression<Func<T, bool>>? expression = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

        IQueryable<T> GetQueryable(Expression<Func<T, bool>>? expression = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

        bool AddEntity(T entity);

        bool AddEntity(IEnumerable<T> entities);

        bool DeleteEntity(T entity);

        bool DeleteEntity(IEnumerable<T> entities);

        bool UpdateEntity(T entity, Expression<Func<T, object>>[]? fields = null);

        bool UpdateEntity(IEnumerable<T> entities, Expression<Func<T, object>>[]? fields = null);
    }
}
