using Microsoft.EntityFrameworkCore.Storage;

namespace FGMS.Core.EfCore.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 提交更改
        /// </summary>
        /// <returns></returns>
        int SaveChanges();

        /// <summary>
        /// 提交更改异步
        /// </summary>
        /// <returns></returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// 开始事务
        /// </summary>
        Task BeginTrans();

        /// <summary>
        /// 提交事务
        /// </summary>
        Task CommitTrans();

        /// <summary>
        /// 回滚事务
        /// </summary>
        Task RollBackTrans();

        ///// <summary>
        ///// 提交更改
        ///// </summary>
        ///// <param name="cancellationToken">取消令牌</param>
        ///// <returns></returns>
        //int SaveChanges(CancellationToken cancellationToken = default);

        ///// <summary>
        ///// 提交更改异步
        ///// </summary>
        ///// <param name="cancellationToken">取消令牌</param>
        ///// <returns></returns>
        //Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        ///// <summary>
        ///// 开始事务
        ///// </summary>
        ///// <param name="cancellationToken">取消令牌</param>
        //Task BeginTrans(CancellationToken cancellationToken = default);

        ///// <summary>
        ///// 提交事务
        ///// </summary>
        ///// <param name="cancellationToken">取消令牌</param>
        //Task CommitTrans(CancellationToken cancellationToken = default);

        ///// <summary>
        ///// 回滚事务
        ///// </summary>
        ///// <param name="cancellationToken">取消令牌</param>
        //Task RollBackTrans(CancellationToken cancellationToken = default);
    }
}
