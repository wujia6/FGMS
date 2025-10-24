using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FGMS.Core.EfCore.Implements
{
    public class FgmsDbRepository<T> : IFgmsDbRepository<T> where T : class
    {
        public FgmsDbRepository(IFgmsDbContext context)
        {
            FamsDbContext = context ?? throw new ArgumentNullException(nameof(context));
            Entities = context.Set<T>().AsNoTracking();
        }

        public IFgmsDbContext FamsDbContext { get; private set; }

        public IQueryable<T> Entities { get; set; }

        public bool Delete(T entity)
        {
            try
            {
                FamsDbContext.Set<T>().AttachRange(entity);
                FamsDbContext.Entry(entity).State = EntityState.Deleted;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Delete(IEnumerable<T> entities)
        {
            try
            {
                if (!entities.Any())
                    return false;
                foreach (var entity in entities)
                {
                    if (!Delete(entity))
                        return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Insert(T entity)
        {
            try
            {
                FamsDbContext.Set<T>().AttachRange(entity);
                FamsDbContext.Entry(entity).State = EntityState.Added;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Insert(IEnumerable<T> entities)
        {
            try
            {
                if (!entities.Any())
                    return false;
                foreach (var entity in entities)
                {
                    if (!Insert(entity))
                        return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Update(T entity, Expression<Func<T, object>>[]? fields = null)
        {
            try
            {
                FamsDbContext.Set<T>().Attach(entity);
                if (fields != null && fields.Any())
                {
                    foreach (var expression in fields)
                    {
                        FamsDbContext.Entry(entity).Property(expression).IsModified = true;
                    }
                }
                else
                    FamsDbContext.Entry(entity).State = EntityState.Modified;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Update(IEnumerable<T> entities, Expression<Func<T, object>>[]? fields = null)
        {
            try
            {
                if (!entities.Any())
                    return false;
                foreach (var entity in entities)
                {
                    if (!Update(entity, fields))
                        return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
