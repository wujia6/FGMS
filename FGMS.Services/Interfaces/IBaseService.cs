using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace FGMS.Services.Interfaces
{
    public interface IBaseService<T> where T : class, new()
    {
        Task<bool> AddAsync(T model);

        Task<bool> AddAsync(List<T> models);

        Task<bool> RemoveAsync(T model);

        Task<bool> RemoveAsync(List<T> models);

        Task<bool> UpdateAsync(T model, Expression<Func<T, object>>[]? fields = null);

        Task<bool> UpdateAsync(List<T> models, Expression<Func<T, object>>[]? fields = null);

        Task<T> ModelAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

        Task<List<T>> ListAsync(Expression<Func<T, bool>>? expression = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

        IQueryable<T> GetQueryable(Expression<Func<T, bool>>? expression = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);
    }
}
