using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Models;
using FGMS.Models.Entities;
using FGMS.Mx.Repositories;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using Microsoft.EntityFrameworkCore;

namespace FGMS.Services.Implements
{
    internal class MaterialIssueOrderService : BaseService<MaterialIssueOrder>, IMaterialIssueOrderService
    {
        private readonly IMaterialIssueOrderRepository materialIssueOrderRepository;
        private readonly IProductionOrderRepository productionOrderRepository;
        private readonly IUserInfoRepository userInfoRepository;
        private readonly IBusinessRepository businessRepository;
        private readonly IFgmsDbContext fgmsDbContext;

        public MaterialIssueOrderService(
            IBaseRepository<MaterialIssueOrder> repo, 
            IProductionOrderRepository productionOrderRepository,
            IUserInfoRepository userInfoRepository,
            IBusinessRepository businessRepository, 
            IFgmsDbContext context) : base(repo, context)
        {
            materialIssueOrderRepository = repo as IMaterialIssueOrderRepository ?? throw new ArgumentNullException(nameof(repo));
            fgmsDbContext = context;
            this.productionOrderRepository = productionOrderRepository;
            this.userInfoRepository = userInfoRepository;
            this.businessRepository = businessRepository;
        }

        public async Task<dynamic> EquipmentReceiveAsync(int mioId, int userInfoId)
        {
            var mio = await materialIssueOrderRepository.GetEntityAsync(
                expression: src => src.Id == mioId, 
                include: src => src.Include(src => src.ProductionOrder!).ThenInclude(src => src.Equipment!).ThenInclude(src => src.Organize!));

            var po = mio?.ProductionOrder;

            if (mio is null || po is null)
                return new { success = false, message = "未知发料单" };

            var currentUser = await userInfoRepository.GetEntityAsync(src => src.Id == userInfoId);
            string orgCode = mio.ProductionOrder!.Equipment!.Organize!.Code;

            if (string.IsNullOrEmpty(currentUser.OperateRange) || !currentUser.OperateRange.Contains(orgCode))
                return new { success = false, message = "无操作权限" };

            await fgmsDbContext.BeginTrans();
            try
            {
                mio.Status = MioStatus.已接收;
                if (!materialIssueOrderRepository.UpdateEntity(mio, new Expression<Func<MaterialIssueOrder, object>>[] { src => src.Status }))
                    throw new Exception($"发料单:{mio.OrderNo}状态更新失败");

                if (mio.Type == MioType.发料)
                {
                    po.Status = ProductionOrderStatus.已收料;
                    if (!productionOrderRepository.UpdateEntity(po, new Expression<Func<ProductionOrder, object>>[] { src => src.Status }))
                        throw new Exception($"制令单:{po.OrderNo}状态更新失败");
                }

                var success = await fgmsDbContext.SaveChangesAsync() > 0;
                if (success)
                    await fgmsDbContext.CommitTrans();
                else
                    await fgmsDbContext.RollBackTrans();
                return new { success, message = success ? "操作成功，状态已更新" : "操作失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception($"Error:{ex.Message}");
            }
        }

        public async Task<dynamic> OutboundAsync(int[] ids, int userInfoId)
        {
            var entities = await materialIssueOrderRepository.GetListAsync(expression: src => ids.Contains(src.Id), include: src => src.Include(src => src.ProductionOrder!));

            if (entities is null || entities.Any(src => src.ProductionOrder is null))
                return await Task.FromResult(new { success = false, message = "未知发料单" });
            
            await fgmsDbContext.BeginTrans();
            try
            {
                foreach (var entity in entities)
                {
                    if (entity.Status != MioStatus.待出库)
                        return new { success = false, message = $"发料单：{entity.OrderNo}状态错误，无法发料" };

                    var productionOrder = entity.ProductionOrder;
                    entity.SendorId = userInfoId;
                    entity.Status = MioStatus.已出库;
                    if (!materialIssueOrderRepository.UpdateEntity(entities, new Expression<Func<MaterialIssueOrder, object>>[] { src => src.SendorId, src => src.Status }))
                        throw new Exception($"发料单:{ entity.OrderNo }状态更新失败");

                    if (entity.Type == MioType.发料)
                    {
                        productionOrder!.Status = ProductionOrderStatus.配送中;
                        if (!productionOrderRepository.UpdateEntity(productionOrder, new Expression<Func<ProductionOrder, object>>[] { src => src.Status }))
                            throw new Exception($"制令单:{ productionOrder.OrderNo }状态更新失败");
                    }
                }
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                if (success)
                    await fgmsDbContext.CommitTrans();
                else
                    await fgmsDbContext.RollBackTrans();
                return new { success, message = success ? "操作成功，状态已更新" : "操作失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception($"Error:{ ex.Message }");
            }
        }

        public async Task<dynamic> PrepareAsync(int[] mioIds)
        {
            var entities = await materialIssueOrderRepository.GetListAsync(src => mioIds.Contains(src.Id), include: src => src.Include(src => src.ProductionOrder!));

            if (!entities.Any())
                return new { success = false, message = "未知发料单集合" };

            await fgmsDbContext.BeginTrans();
            try
            {
                var pcodes = entities.Select(src => src.ProductionOrder!.OrderNo).Distinct().ToList();
                string inputCodes = string.Join(",", pcodes.Select(s => $"'{s}'"));
                var barCodes = await businessRepository.GetBarcodesAsync(inputCodes);
                foreach (var entity in entities)
                {
                    if (entity.Status != MioStatus.待备料)
                        return new { success = false, message = $"发料单：{entity.OrderNo}状态错误，无法备料" };

                    string pcode = entity.ProductionOrder!.OrderNo;
                    string barCode = barCodes.FirstOrDefault(src => src.ProductionOrderCode == pcode)?.BarCode ?? string.Empty;

                    if (string.IsNullOrEmpty(barCode))
                        return new { success = false, message = $"墨心制令单：{pcode}未生成出库物料条码" };

                    entity.MxBarCode = barCode;
                    entity.Status = MioStatus.分拣中;
                }
                bool success = materialIssueOrderRepository.UpdateEntity(entities, new Expression<Func<MaterialIssueOrder, object>>[] { src => src.Status, src => src.MxBarCode }) && await fgmsDbContext.SaveChangesAsync() > 0;
                if (success)
                    await fgmsDbContext.CommitTrans();
                else
                    await fgmsDbContext.RollBackTrans();
                return new { success, message = success ? "操作成功，状态已更新" : "操作失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception($"Error：{ex.Message}");
            }
        }
    }
}
