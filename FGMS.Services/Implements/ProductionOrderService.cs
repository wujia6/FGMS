using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Models;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using Microsoft.EntityFrameworkCore;

namespace FGMS.Services.Implements
{
    internal class ProductionOrderService : BaseService<ProductionOrder>, IProductionOrderService
    {
        //private readonly IFgmsDbContext fgmsDbContext;
        //private readonly IProductionOrderRepository productionOrderRepository;
        //private readonly IMaterialIssueOrderRepository materialIssueOrderRepository;
        //private readonly IWorkOrderRepository workOrderRepository;
        //private readonly GenerateRandomNumber randomNumber;

        public ProductionOrderService(IBaseRepository<ProductionOrder> repo, IFgmsDbContext context) : base(repo, context)
        {
        }

        //public ProductionOrderService(
        //    IFgmsDbContext context,
        //    IBaseRepository<ProductionOrder> repo,
        //    IMaterialIssueOrderRepository materialIssueOrderRepository,
        //    IWorkOrderRepository workOrderRepository,
        //    GenerateRandomNumber randomNumber) : base(repo, context)
        //{
        //    fgmsDbContext = context ?? throw new ArgumentNullException(nameof(context));
        //    productionOrderRepository = repo as IProductionOrderRepository ?? throw new ArgumentNullException(nameof(repo));
        //    this.materialIssueOrderRepository = materialIssueOrderRepository ?? throw new ArgumentNullException(nameof(materialIssueOrderRepository));
        //    this.workOrderRepository = workOrderRepository ?? throw new ArgumentNullException(nameof(workOrderRepository));
        //    this.randomNumber = randomNumber ?? throw new ArgumentNullException(nameof(randomNumber));
        //}

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
