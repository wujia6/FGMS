using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Models;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FGMS.Services.Implements
{
    internal class ProductionOrderService : BaseService<ProductionOrder>, IProductionOrderService
    {
        private readonly IFgmsDbContext fgmsDbContext;
        private readonly IProductionOrderRepository productionOrderRepository;
        private readonly IEquipmentRepository equipmentRepository;
        private readonly IMaterialIssueOrderRepository materialIssueOrderRepository;
        private readonly IWorkOrderRepository workOrderRepository;
        private readonly IComponentRepository componentRepository;
        private readonly IElementEntityRepository elementEntityRepository;

        public ProductionOrderService(
            IFgmsDbContext context, 
            IBaseRepository<ProductionOrder> repo,
            IEquipmentRepository equipmentRepository,
            IMaterialIssueOrderRepository materialIssueOrderRepository,
            IWorkOrderRepository workOrderRepository,
            IComponentRepository componentRepository,
            IElementEntityRepository elementEntityRepository) : base(repo, context)
        {
            fgmsDbContext = context ?? throw new ArgumentNullException(nameof(context));
            productionOrderRepository = repo as IProductionOrderRepository ?? throw new ArgumentNullException(nameof(repo));
            this.equipmentRepository = equipmentRepository;
            this.materialIssueOrderRepository = materialIssueOrderRepository;
            this.workOrderRepository = workOrderRepository;
            this.componentRepository = componentRepository;
            this.elementEntityRepository = elementEntityRepository;
        }

        public async Task<dynamic> MadeBeginAsync(int poid)
        {
            var productionOrder = await productionOrderRepository.GetEntityAsync(
                expression: x => x.Id == poid, include: 
                src => src.Include(src => src.Equipment!).Include(src => src.WorkOrder!).ThenInclude(src => src.Components!).ThenInclude(src => src.ElementEntities!));

            var equipment = productionOrder.Equipment;

            if (productionOrder == null)
                return new { success = false, message = "未知制令单" };

            if (equipment == null)
                return new { success = false, message = "未知设备信息" };

            if (equipment.PoMount)
                return new { success = false, message = "不能同时开工多个制令单" };

            if (productionOrder.IsDc!.Value && !productionOrder.Report!.Value)
                return new { success = false, message = $"制令单：{productionOrder.OrderNo}前制工序未完工" };

            if (!productionOrder.IsDc!.Value && productionOrder.Status != ProductionOrderStatus.已收料)
                return new { success = false, message = "请按流程叫料、收料后，再开工" };

            if (productionOrder.WorkOrder! != null)
            {
                if (productionOrder.WorkOrder!.Status != WorkOrderStatus.机台接收)
                    return new { success = false, message = $"砂轮工单：{productionOrder.WorkOrder!.OrderNo}未接收" };

                if (productionOrder.WorkOrder!.Status == WorkOrderStatus.挂起)
                    return new { success = false, message = $"砂轮工单：{productionOrder.WorkOrder!.OrderNo}已挂起，无法开工" };
                if (productionOrder.WorkOrder!.Components != null && 
                    productionOrder.WorkOrder!.Components.Any(src => src.ElementEntities != null && src.ElementEntities.Any(e => e.Status != ElementEntityStatus.上机)))
                    return new { success = false, message = "砂轮未上机" };
            }

            await fgmsDbContext.BeginTrans();
            try
            {
                productionOrder.Status = ProductionOrderStatus.生产中;
                productionOrderRepository.UpdateEntity(productionOrder, new Expression<Func<ProductionOrder, object>>[] { x => x.Status });
                equipment.PoMount = true;
                equipmentRepository.UpdateEntity(equipment, new Expression<Func<Equipment, object>>[] { x => x.PoMount });
                bool success = await fgmsDbContext.SaveChangesAsync() >= 2;
                if (success)
                    await fgmsDbContext.CommitTrans();
                else
                    await fgmsDbContext.RollBackTrans();
                return success ? new { success, message = "操作成功，制令单已开工", data = productionOrder.OrderNo } : new { success, message = "操作失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception("Error:" + ex.Message);
            }
        }

        //public async Task<dynamic> EquipmentChangeAsync(int poId, int equId, string oldEquCode, string reason, int userInfoId)
        //{
        //    var productionOrder = await productionOrderRepository.GetEntityAsync(
        //        expression: src => src.Id == poId,
        //        include: src => src.Include(src => src.WorkOrder!).Include(src => src.MaterialIssueOrders!).Include(src => src.Equipment!));

        //    if (productionOrder is null)
        //        return new { success = false, message = "未知制令单" };

        //    if (productionOrder.WorkOrder != null && productionOrder.WorkOrder.Status != WorkOrderStatus.机台接收)
        //        return new { success = false, message = "已申请砂轮，请接收后再创建变更申请" };

        //    if (productionOrder.MaterialIssueOrders!.Any(src => src.Type == MioType.发料 && src.Status != MioStatus.已接收))
        //        return new { success = false, message = "已申请发料，请接收后再创建变更申请" };

        //    await fgmsDbContext.BeginTrans();
        //    try
        //    {
        //        //添加机台变更单
        //        equipmentChangeOrderRepository.AddEntity(new EquipmentChangeOrder
        //        {
        //            ProductionOrderId = poId,
        //            UserInfoId = userInfoId,
        //            EquipmentId = equId,
        //            OldEquipmentCode = oldEquCode,
        //            Reason = reason
        //        });
        //        //更新制令单状态
        //        productionOrder.Status = ProductionOrderStatus.已暂停;
        //        productionOrderRepository.UpdateEntity(productionOrder, new Expression<Func<ProductionOrder, object>>[] { src => src.Status });
        //        bool saved = await fgmsDbContext.SaveChangesAsync() >= 2;
        //        if (!saved)
        //        {
        //            await fgmsDbContext.RollBackTrans();
        //            return new { success = false, message = "机台变更申请创建失败" };
        //        }
        //        await fgmsDbContext.CommitTrans();
        //        return new { success = true, message = "机台变更申请创建成功" };
        //    }
        //    catch (Exception ex)
        //    {
        //        await fgmsDbContext.RollBackTrans();
        //        throw new Exception($"创建机台变更申请出现错误：{ex.Message}");
        //    }
        //}

        public async Task<dynamic> CascadeRemoveAsync(string poNo)
        {
            // 1. 加载主实体及关联数据
            var entity = await productionOrderRepository.GetEntityAsync(
                expression: op => op.OrderNo.Equals(poNo),
                include: src => src.Include(x => x.WorkOrder!).ThenInclude(wo => wo.Components).Include(x => x.MaterialIssueOrders!));

            if (entity == null)
                return new { success = false, message = "未知制令单" };

            var issueOrders = entity.MaterialIssueOrders;
            var workOrder = entity.WorkOrder!;

            await fgmsDbContext.BeginTrans();
            try
            {
                // 2. 删除发料单
                if (issueOrders?.Any() == true)
                    materialIssueOrderRepository.DeleteEntity(issueOrders);

                // 3. 回收砂轮组
                if (workOrder?.Components?.Any() == true)
                {
                    foreach (var comp in workOrder.Components)
                    {
                        if (comp.CargoSpaceHistory is null)
                        {
                            await fgmsDbContext.RollBackTrans();
                            return new { success = false, message = $"砂轮组 {comp.Code} 未记录仓位信息，无法入库" };
                        }

                        comp.WorkOrderId = null;
                        comp.CargoSpaceId = comp.CargoSpaceHistory!.Value;
                        comp.Status = ElementEntityStatus.在库;
                        componentRepository.UpdateEntity(comp, new Expression<Func<Component, object>>[] 
                        { 
                            src => src.WorkOrderId,
                            src => src.CargoSpaceId,
                            src => src.Status
                        });

                        // 回收砂轮元件
                        if (comp.ElementEntities != null && comp.ElementEntities.Any())
                        {
                            foreach (var elem in comp.ElementEntities)
                            {
                                elem.CargoSpaceId = comp.CargoSpaceId;
                                elem.Status = ElementEntityStatus.在库;
                                elem.Position = null;
                                elementEntityRepository.UpdateEntity(elem, new Expression<Func<ElementEntity, object>>[]
                                {
                                    src => src.CargoSpaceId,
                                    src => src.Status,
                                    src => src.Position
                                });
                            }
                        }
                    }
                }

                // 4. 删除砂轮工单
                if (workOrder != null)
                    workOrderRepository.DeleteEntity(workOrder);

                // 5. 删除制令单
                productionOrderRepository.DeleteEntity(entity);

                // 6. 保存变更
                var saved = await fgmsDbContext.SaveChangesAsync() > 0;
                if (saved)
                {
                    await fgmsDbContext.CommitTrans();
                    return new { success = true, message = "制令单删除成功" };
                }
                else
                {
                    await fgmsDbContext.RollBackTrans();
                    return new { success = false, message = "制令单删除失败" };
                }
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception($"删除制令单出现错误：{ex.Message}", ex);
            }
        }
    }
}
