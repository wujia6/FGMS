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

        public ProductionOrderService(IFgmsDbContext context, IBaseRepository<ProductionOrder> repo, IEquipmentRepository equipmentRepository) : base(repo, context)
        {
            fgmsDbContext = context ?? throw new ArgumentNullException(nameof(context));
            productionOrderRepository = repo as IProductionOrderRepository ?? throw new ArgumentNullException(nameof(repo));
            this.equipmentRepository = equipmentRepository;
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

            if (!productionOrder.IsDc!.Value && productionOrder.Status != ProductionOrderStatus.已收料)
                return new { success = false, message = "请按流程叫料、收料后，再开工" };

            if (productionOrder.WorkOrder != null)
            {
                if (productionOrder.WorkOrder.Status != WorkOrderStatus.机台接收)
                    return new { success = false, message = $"砂轮工单：{productionOrder.WorkOrder.OrderNo}状态错误" };

                if (productionOrder.WorkOrder.Components != null && productionOrder.WorkOrder.Components.Any(src => src.ElementEntities != null && src.ElementEntities.Any(e => e.Status != ElementEntityStatus.上机)))
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

        //public async Task<dynamic> CreateWheelAndMaterialAsync(int poid, int? qty)
        //{
        //    await fgmsDbContext.BeginTrans();
        //    try
        //    {
        //        var poModel = await productionOrderRepository!.GetEntityAsync(expression:x => x.Id == poid, include: src => src.Include(src => src.MaterialIssueOrders!));

        //        if (poModel == null)
        //            return new { success = false, message = "未知制令单" };

        //        if (poModel.MaterialIssueOrders!.Count() == 2)
        //            return new { success = false, message = "制令单超出发料次数限制" };

        //        var mioEntity = new MaterialIssueOrder
        //        {
        //            ProductionOrderId = poModel.Id,
        //            OrderNo = $"MIO{randomNumber!.CreateOrderNum()}",
        //            MaterialNo = poModel.MaterialCode,
        //            MaterialName = poModel.MaterialName,
        //            MaterialSpce = poModel.MaterialSpec
        //        };

        //        switch (poModel.Status)
        //        {
        //            case ProductionOrderStatus.已排配:
        //                poModel.Status = ProductionOrderStatus.待发料;
        //                mioEntity.Type = MioType.发料;
        //                mioEntity.Quantity = poModel.Quantity;
        //                //创建砂轮工单
        //                var wheelOrder = new WorkOrder
        //                {
        //                    ProductionOrderId = poModel.Id,
        //                    OrderNo = $"WO{randomNumber.CreateOrderNum()}",
        //                    UserInfoId = poModel.UserInfoId,
        //                    Type = WorkOrderType.砂轮申领,
        //                    Priority = WorkOrderPriority.低,
        //                    MaterialNo = poModel.MaterialCode,
        //                    MaterialSpec = poModel.MaterialSpec,
        //                    Status = WorkOrderStatus.待审,
        //                    RequiredDate = DateTime.Now.AddHours(4),
        //                    Remark = poModel.Remark
        //                };
        //                workOrderRepository.AddEntity(wheelOrder);
        //                bool saved = await fgmsDbContext.SaveChangesAsync() > 0;
        //                if (!saved)
        //                {
        //                    await fgmsDbContext.RollBackTrans();
        //                    return new { success = false, message = "砂轮工单创建失败" };
        //                }
        //                poModel.WorkOrderId = wheelOrder.Id;
        //                productionOrderRepository.UpdateEntity(poModel, new Expression<Func<ProductionOrder, object>>[] { x => x.WorkOrderId, x => x.Status });
        //                break;
        //            case ProductionOrderStatus.已收料:
        //            case ProductionOrderStatus.生产中:
        //                mioEntity.Type = MioType.补料;
        //                mioEntity.Quantity = qty!.Value;
        //                break;
        //            default:
        //                return new { success = false, message = "制令单状态错误，无法操作" };
        //        }
        //        materialIssueOrderRepository.AddEntity(mioEntity);
        //        bool success = await fgmsDbContext.SaveChangesAsync() > 0;
        //        if (success)
        //            await fgmsDbContext.CommitTrans();
        //        else
        //            await fgmsDbContext.RollBackTrans();
        //        return success ? new { success, message = mioEntity.Type == MioType.发料 ? "已创建砂轮工单与发料单，操作成功" : "已创建补料单，操作成功" } : new { success, message = "操作失败" };
        //    }
        //    catch (Exception ex)
        //    {
        //        await fgmsDbContext.RollBackTrans();
        //        throw new Exception("Error:" + ex.Message);
        //    }
        //}
    }
}
