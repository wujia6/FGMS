using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

namespace FGMS.Services.Implements
{
    internal class BaseService<T> : IBaseService<T> where T : class, new()
    {
        private readonly IBaseRepository<T> repo;
        private readonly IFgmsDbContext context;

        public BaseService(IBaseRepository<T> repo, IFgmsDbContext context)
        {
            this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> AddAsync(T model)
        {
            return repo.AddEntity(model) && await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddAsync(List<T> models)
        {
            return repo.AddEntity(models) && await context.SaveChangesAsync() > 0;
        }

        public async Task<List<T>> ListAsync(Expression<Func<T, bool>>? expression = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            var result = await repo.GetListAsync(expression, include);
            return result.ToList();
        }

        public async Task<T> ModelAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            return await repo.GetEntityAsync(expression, include);
        }

        public async Task<bool> RemoveAsync(T model)
        {
            return repo.DeleteEntity(model) && await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveAsync(List<T> models)
        {
            return repo.DeleteEntity(models) && await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(T model, Expression<Func<T, object>>[]? fields = null)
        {
            return repo.UpdateEntity(model, fields) && await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(List<T> models, Expression<Func<T, object>>[]? fields = null)
        {
            return repo.UpdateEntity(models, fields) && await context.SaveChangesAsync() > 0;
        }
    }
}
