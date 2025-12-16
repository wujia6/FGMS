using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace FGMS.Utils
{
    public class QueryableBuilder<T>
    {
        private IQueryable<T> query;

        public QueryableBuilder(IQueryable<T> baseQuery)
        {
            query = baseQuery;
        }

        public QueryableBuilder<T> Where(Expression<Func<T, bool>>? expression)
        {
            if (expression != null)
                query = query.Where(expression);
            return this;
        }

        public QueryableBuilder<T> Include(Func<IQueryable<T>, IIncludableQueryable<T, object>>? include)
        {
            if (include != null)
                query = include(query);
            return this;
        }

        public IQueryable<T> Build() => query;

        public async Task<IEnumerable<T>> ToListAsync() => await query.ToListAsync();
    }
}
