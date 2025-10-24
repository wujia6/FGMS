using System.Linq.Expressions;

namespace FGMS.Core.EfCore.Interfaces
{
    public interface IFgmsDbRepository<T> where T : class
    {
        /// <summary>
        /// 上下文对象
        /// </summary>
        IFgmsDbContext FamsDbContext { get; }
        /// <summary>
        /// 可查询实体集合
        /// </summary>
        IQueryable<T> Entities { get; set; }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool Insert(T entity);
        /// <summary>
        /// 添加集合
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        bool Insert(IEnumerable<T> entities);
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool Delete(T entity);
        /// <summary>
        /// 删除集合
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        bool Delete(IEnumerable<T> entities);
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        bool Update(T entity, Expression<Func<T, object>>[]? fields = null);
        /// <summary>
        /// 更新集合
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        bool Update(IEnumerable<T> entities, Expression<Func<T, object>>[]? fields = null);
    }
}
