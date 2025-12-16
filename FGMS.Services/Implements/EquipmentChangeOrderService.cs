using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Models;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FGMS.Services.Implements
{
    internal class EquipmentChangeOrderService : BaseService<EquipmentChangeOrder>, IEquipmentChangeOrderService
    {
        private readonly IEquipmentChangeOrderRepository equipmentChangeOrderRepository;
        private readonly IProductionOrderRepository productionOrderRepository;
        private readonly IFgmsDbContext fgmsDbContext;
        private readonly IWorkOrderRepository workOrderRepository;

        public EquipmentChangeOrderService(
            IWorkOrderRepository workOrderRepository,
            IProductionOrderRepository productionOrderRepository,
            IBaseRepository<EquipmentChangeOrder> repo,
            IFgmsDbContext context) : base(repo, context)
        {
            this.workOrderRepository = workOrderRepository;
            this.productionOrderRepository = productionOrderRepository;
            equipmentChangeOrderRepository = repo as IEquipmentChangeOrderRepository ?? throw new ArgumentNullException(nameof(repo));
            fgmsDbContext = context;
        }

        public async Task<dynamic> AuditAsync(int ecId, string status)
        {
            var ecEntity = await equipmentChangeOrderRepository.GetEntityAsync(expression: src => src.Id == ecId, include: src => src.Include(src => src.ProductionOrder!).ThenInclude(src => src.WorkOrder!));
            var poEntity = ecEntity.ProductionOrder;
            var woEntity = ecEntity.ProductionOrder?.WorkOrder;

            if (ecEntity == null || poEntity == null || woEntity == null)
                return new { success = false, message = "未知机台更改单，或缺少关联数据" };

            await fgmsDbContext.BeginTrans();
            try
            {
                ecEntity.Status = Enum.Parse<WorkOrderStatus>(status);
                equipmentChangeOrderRepository.UpdateEntity(ecEntity, new Expression<Func<EquipmentChangeOrder, object>>[] { src => src.Status });
                if (ecEntity.Status == WorkOrderStatus.审核通过)
                {
                    poEntity.EquipmentId = ecEntity.EquipmentId;
                    productionOrderRepository.UpdateEntity(poEntity, new Expression<Func<ProductionOrder, object>>[] { src => src.EquipmentId });
                }
                woEntity.Status = WorkOrderStatus.机台变更;
                workOrderRepository.UpdateEntity(woEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                bool saved = await fgmsDbContext.SaveChangesAsync() > 0;
                if (!saved)
                {
                    await fgmsDbContext.RollBackTrans();
                    return new { success = false, message = "操作失败" };
                }
                await fgmsDbContext.CommitTrans();
                return new { success = true, message = "操作成功" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception($"审核时发生错误：{ex.Message}");
            }
        }

        public async Task<dynamic> CreateAsync(int woId, int equipmentId, string oldEquipmentCode, string reason, int userId)
        {
            var woEntity = await workOrderRepository.GetEntityAsync(
                expression: src => src.Id == woId,
                include: src => src.Include(src => src.ProductionOrder!).Include(src => src.Components!).ThenInclude(src => src.ElementEntities!));

            if (woEntity == null || woEntity.ProductionOrder == null)
                return new { success = false, message = "未知砂轮工单，或未知制令单" };

            if (woEntity.Status != WorkOrderStatus.机台接收)
                return new { success = false, message = "砂轮工单状态错误，请先接收" };

            if (woEntity.Components!.Any(src => src.ElementEntities!.Any(src => src.Status != ElementEntityStatus.出库 && src.Status != ElementEntityStatus.下机)))
                return new { success = false, message = "砂轮工件状态错误，只能是出库或下机状态" };

            await fgmsDbContext.BeginTrans();
            try
            {
                //创建机台更改
                var ecOrder = new EquipmentChangeOrder
                {
                    ProductionOrderId = woEntity.ProductionOrder!.Id,
                    UserInfoId = userId,
                    EquipmentId = equipmentId,
                    OldEquipmentCode = oldEquipmentCode,
                    Reason = reason
                };
                //更新砂轮工单状态
                woEntity.Status = WorkOrderStatus.挂起;
                equipmentChangeOrderRepository.AddEntity(ecOrder);
                workOrderRepository.UpdateEntity(woEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                bool saved = await fgmsDbContext.SaveChangesAsync() > 0;
                if (!saved)
                {
                    await fgmsDbContext.RollBackTrans();
                    return new { success = false, message = "机台更改创建失败" };
                }
                await fgmsDbContext.CommitTrans();
                return new { success = true, message = "机台更改创建成功" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception($"创建机台更改出现错误：{ex.Message}");
            }
        }
    }
}