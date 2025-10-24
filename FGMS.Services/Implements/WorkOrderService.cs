using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Models;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FGMS.Services.Implements
{
    internal class WorkOrderService : BaseService<WorkOrder>, IWorkOrderService
    {
        private readonly IWorkOrderRepository? workOrderRepository;
        private readonly IFgmsDbContext? fgmsDbContext;
        private readonly IComponentRepository? componentRepository;
        private readonly IElementEntityRepository? elementEntityRepository;
        private readonly ICargoSpaceRepository? cargoSpaceRepositroy;
        private readonly IWorkOrderStandardRepository? workOrderStandardRepository;
        private readonly ITrackLogRepository? logRepository;
        private readonly GenerateRandomNumber? randomNumber;

        public WorkOrderService(IBaseRepository<WorkOrder> repo, IFgmsDbContext context) : base(repo, context)
        {
        }

        public WorkOrderService(
            IBaseRepository<WorkOrder> repo,
            IFgmsDbContext context,
            IComponentRepository componentRepository,
            IElementEntityRepository elementEntityRepository,
            ICargoSpaceRepository cargoSpaceRepositroy,
            IWorkOrderStandardRepository workOrderStandardRepository,
            ITrackLogRepository logRepository,
            GenerateRandomNumber randomNumber) : base(repo, context)
        {
            this.workOrderRepository = repo as IWorkOrderRepository;
            this.fgmsDbContext = context;
            this.componentRepository = componentRepository;
            this.elementEntityRepository = elementEntityRepository;
            this.cargoSpaceRepositroy = cargoSpaceRepositroy;
            this.workOrderStandardRepository = workOrderStandardRepository;
            this.logRepository = logRepository;
            this.randomNumber = randomNumber;
        }

        public async Task<dynamic> ReceiveAsync(dynamic paramJson)
        {
            await fgmsDbContext!.BeginTrans();
            try
            {
                int woId = paramJson!.woId;
                var orderEntity = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == woId);

                if (orderEntity == null)
                    return new { success = false, message = "未知工单" };

                var expression = new List<Expression<Func<WorkOrder, object>>> { src => src.Status };
                switch (orderEntity.Type)
                {
                    case WorkOrderType.砂轮返修:
                        orderEntity.Status = WorkOrderStatus.参数修整;
                        orderEntity.AgvTaskCode = Guid.NewGuid().ToString("N")[..16];
                        expression.Add(src => src.AgvTaskCode);
                        break;
                    case WorkOrderType.砂轮退仓:
                        //入库
                        var cmps = await componentRepository!.GetListAsync(expression: src => src.WorkOrderId == orderEntity.Id, include: src => src.Include(src => src.ElementEntities!));
                        foreach (var cmp in cmps)
                        {
                            if (!cmp.CargoSpaceHistory.HasValue)
                            {
                                return cmp.IsStandard ? new { success = false, message = $"标组：{cmp.Code} 未找到相关货位，入库失败" } : new { success = false, message = $"非标组未找到相关货位，入库失败" };
                            }
                            cmp.CargoSpaceId = cmp.CargoSpaceHistory.Value;
                            cmp.WorkOrderId = null;
                            cmp.Status = ElementEntityStatus.在库;
                            componentRepository.UpdateEntity(cmp, new Expression<Func<Component, object>>[] { src => src.Status, src => src.CargoSpaceId, src => src.WorkOrderId });
                            //更新工件状态
                            var ees = cmp.ElementEntities!.ToList();
                            ees.ForEach(ee =>
                            {
                                ee.CargoSpaceId = cmp.CargoSpaceId;
                                ee.Status = ElementEntityStatus.在库;
                            });
                            elementEntityRepository!.UpdateEntity(ees, new Expression<Func<ElementEntity, object>>[] { src => src.Status, src => src.CargoSpaceId! });
                        }
                        orderEntity.Status = WorkOrderStatus.工单结束;
                        break;
                    default: break;
                }
                workOrderRepository.UpdateEntity(orderEntity, expression.ToArray());
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                await fgmsDbContext.CommitTrans();
                return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception("Error Updating Order Status" + ex.Message);
            }
        }

        public async Task<dynamic> CancelAsync(int orderId)
        {
            var order = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == orderId, include: src => src.Include(src => src.Components!).ThenInclude(src => src.ElementEntities!));

            if (order == null)
                return new { success = false, message = "工单不存在" };

            if (order.Status == WorkOrderStatus.待审 || order.Status == WorkOrderStatus.审核通过 || order.Status == WorkOrderStatus.砂轮整备 || order.Status == WorkOrderStatus.参数修整 || order.Status == WorkOrderStatus.整备完成)
            {
                await fgmsDbContext!.BeginTrans();
                try
                {
                    if (order.Components != null && order.Components.Any())
                    {
                        foreach (var cmp in order.Components)
                        {
                            if (cmp.IsStandard)
                            {
                                if (!cmp.CargoSpaceHistory.HasValue)
                                {
                                    return new { success = false, message = $"标组：{cmp.Code}未找到相关货位，入库失败" };
                                }
                                cmp.CargoSpaceId = cmp.CargoSpaceHistory.Value;
                                cmp.WorkOrderId = new int?();
                                cmp.Status = ElementEntityStatus.在库;
                                componentRepository!.UpdateEntity(cmp, new Expression<Func<Component, object>>[] { src => src.Status, src => src.CargoSpaceId!, src => src.WorkOrderId! });

                                if (cmp.ElementEntities == null) continue;
                                var ees = cmp.ElementEntities.ToList();
                                ees.ForEach(ee =>
                                {
                                    ee.CargoSpaceId = cmp.CargoSpaceId;
                                    ee.Status = ElementEntityStatus.在库;
                                });
                                elementEntityRepository!.UpdateEntity(ees, new Expression<Func<ElementEntity, object>>[] { src => src.Status, src => src.CargoSpaceId! });
                            }
                            else
                            {
                                if (cmp.ElementEntities == null) continue;
                                foreach (var ee in cmp.ElementEntities)
                                {
                                    if (!ee.CargoSpaceHistory.HasValue)
                                    {
                                        return new { success = false, message = $"工件：{ee.MaterialNo}未找到相关货位，入库失败" };
                                    }
                                    ee.ComponentId = new int?();
                                    ee.CargoSpaceId = ee.CargoSpaceHistory.Value;
                                    ee.IsGroup = false;
                                    ee.Status = ElementEntityStatus.在库;
                                    elementEntityRepository!.UpdateEntity(ee, new Expression<Func<ElementEntity, object>>[] { src => src.Status, src => src.CargoSpaceId!, src => src.ComponentId!, src => src.IsGroup });
                                }
                                componentRepository!.DeleteEntity(cmp);
                            }
                        }
                    }
                    order.Status = WorkOrderStatus.取消;
                    workOrderRepository.UpdateEntity(order, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                    bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                    await fgmsDbContext.CommitTrans();
                    return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
                }
                catch (Exception ex)
                {
                    await fgmsDbContext.RollBackTrans();
                    throw new Exception("Error Updating Order Status" + ex.Message);
                }
            }
            else
                return new { success = false, message = "无法取消，工单已进入生产流程。请按退仓流程操作" };
        }

        public async Task<dynamic> ReadyAsync(dynamic paramJson)
        {
            int woId = paramJson.woId;
            var order = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == woId);

            if (order is null)
                return new { success = false, message = "未知工单" };

            await fgmsDbContext!.BeginTrans();
            try
            {
                //添加标准组
                int[] stdCmpIds = JsonConvert.DeserializeObject<int[]>(paramJson.stdCmpIds.ToString());
                if (stdCmpIds.Length > 0)
                {
                    var stdCmps = await componentRepository!.GetListAsync(expression: src => stdCmpIds.Contains(src.Id), include: src => src.Include(src => src.ElementEntities!));
                    foreach (var cmp in stdCmps)
                    {
                        if (cmp.Status != ElementEntityStatus.在库)
                            return new { success = false, message = $"标准组：{cmp.Code} 状态异常！" };

                        //修改标准组工件状态
                        foreach (var ee in cmp.ElementEntities!)
                        {
                            if (ee.Status != ElementEntityStatus.在库)
                                return new { success = false, message = $"标准组工件：{ee.Code} 状态异常！" };

                            ee.CargoSpaceId = new int?();
                            ee.Status = ElementEntityStatus.出库;
                            elementEntityRepository!.UpdateEntity(ee, new Expression<Func<ElementEntity, object>>[] { src => src.CargoSpaceId!, src => src.Status });
                        }
                        //修改标准组状态
                        cmp.WorkOrderId = woId;
                        cmp.CargoSpaceId = new int?();
                        cmp.Status = ElementEntityStatus.出库;
                        componentRepository.UpdateEntity(cmp, new Expression<Func<Component, object>>[] { src => src.WorkOrderId!, src => src.CargoSpaceId!, src => src.Status });
                    }
                }
                //添加非标组
                List<dynamic> nonStdCmps = JsonConvert.DeserializeObject<List<dynamic>>(paramJson.nonStdCmps.ToString());
                if (nonStdCmps != null && nonStdCmps.Count > 0)
                {
                    foreach (var cmp in nonStdCmps)
                    {
                        List<dynamic> eeIds = JsonConvert.DeserializeObject<List<dynamic>>(cmp.eeIds.ToString());
                        var intIds = eeIds.Select(src => (int)src.id).ToArray();
                        var ees = await elementEntityRepository!.GetListAsync(expression: src => intIds.Contains(src.Id));
                        foreach (var ee in ees)
                        {
                            if (ee.Status != ElementEntityStatus.在库)
                                return new { success = false, message = $"非标组工件：{ee.Code} 状态异常！" };
                            ee.ComponentId = cmp.cmpId;
                            ee.IsGroup = true;
                            ee.CargoSpaceId = new int?();
                            ee.Status = ElementEntityStatus.出库;
                            ee.Position = eeIds.FirstOrDefault(src => (int)src.id == ee.Id)!.position.ToString();
                            elementEntityRepository.UpdateEntity(ee, new Expression<Func<ElementEntity, object>>[] 
                            { 
                                src => src.ComponentId!, 
                                src => src.IsGroup, 
                                src => src.CargoSpaceId!, 
                                src => src.Status, 
                                src => src.Position 
                            });
                        }
                    }
                }
                order.Status = WorkOrderStatus.参数修整;
                workOrderRepository.UpdateEntity(order, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                await fgmsDbContext.CommitTrans();
                return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception("Error Updating Order Status" + ex.Message);
            }
        }

        public async Task<dynamic> ReadyActionAsync(dynamic paramJson)
        {
            int woId = paramJson.woId;
            var order = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == woId);

            if (order is null)
                return new { success = false, message = "未知工单" };

            await fgmsDbContext!.BeginTrans();
            try
            {
                //添加标准组
                List<dynamic> stdCmps = JsonConvert.DeserializeObject<List<dynamic>>(paramJson.stdCmps.ToString());
                if (stdCmps != null && stdCmps.Count > 0)
                {
                    int[] jsonCmpIds = stdCmps.Select(src => (int)src.cmpId).ToArray();
                    var cmps = await componentRepository!.GetListAsync(expression: src => jsonCmpIds.Contains(src.Id), include: src => src.Include(src => src.ElementEntities!));
                    foreach (var cmp in cmps)
                    {
                        if (cmp.Status != ElementEntityStatus.在库)
                            return new { success = false, message = $"标准组：{cmp.Code} 状态异常！" };

                        var jsonCmp = stdCmps.FirstOrDefault(src => (int)src.cmpId == cmp.Id);
                        List<dynamic> jsonEes = JsonConvert.DeserializeObject<List<dynamic>>(jsonCmp!.ees.ToString());

                        //修改标准组工件状态
                        foreach (var ee in cmp.ElementEntities!)
                        {
                            if (ee.Status != ElementEntityStatus.在库)
                                return new { success = false, message = $"标准组工件：{ee.Code} 状态异常！" };

                            var jsonEe = jsonEes.FirstOrDefault(src => (int)src.id == ee.Id) ?? null;

                            if (jsonEe == null)
                                return new { success = false, message = $"标准组 {cmp.Code} 工件 {ee.Code} 未提供序列位" };

                            ee.CargoSpaceId = new int?();
                            ee.Status = ElementEntityStatus.出库;
                            ee.Position = jsonEe.position;
                            elementEntityRepository!.UpdateEntity(ee, new Expression<Func<ElementEntity, object>>[] { src => src.CargoSpaceId!, src => src.Status, src => src.Position });
                        }

                        //修改标准组状态
                        cmp.WorkOrderId = woId;
                        cmp.CargoSpaceId = new int?();
                        cmp.Status = ElementEntityStatus.出库;
                        componentRepository.UpdateEntity(cmp, new Expression<Func<Component, object>>[] { src => src.WorkOrderId!, src => src.CargoSpaceId!, src => src.Status });
                    }
                }
                //添加非标组
                List<dynamic> nonStdCmps = JsonConvert.DeserializeObject<List<dynamic>>(paramJson.nonStdCmps.ToString());
                if (nonStdCmps != null && nonStdCmps.Count > 0)
                {
                    foreach (var cmp in nonStdCmps)
                    {
                        List<dynamic> eeIds = JsonConvert.DeserializeObject<List<dynamic>>(cmp.eeIds.ToString());
                        var intIds = eeIds.Select(src => (int)src.id).ToArray();
                        var ees = await elementEntityRepository!.GetListAsync(expression: src => intIds.Contains(src.Id));
                        foreach (var ee in ees)
                        {
                            if (ee.Status != ElementEntityStatus.在库)
                                return new { success = false, message = $"非标组工件：{ee.Code} 状态异常！" };

                            var jsonEe = eeIds.FirstOrDefault(src => (int)src.id == ee.Id);

                            if (jsonEe is null || jsonEe.position is null)
                                return new { success = false, message = $"工件：{ee.Code} 未提供序列位" };

                            ee.ComponentId = cmp.cmpId;
                            ee.IsGroup = true;
                            ee.CargoSpaceId = new int?();
                            ee.Status = ElementEntityStatus.出库;
                            ee.Position = jsonEe.position.ToString();
                            elementEntityRepository.UpdateEntity(ee, new Expression<Func<ElementEntity, object>>[]
                            {
                                src => src.ComponentId!,
                                src => src.IsGroup,
                                src => src.CargoSpaceId!,
                                src => src.Status,
                                src => src.Position
                            });
                        }
                    }
                }
                order.Status = WorkOrderStatus.参数修整;
                workOrderRepository.UpdateEntity(order, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                await fgmsDbContext.CommitTrans();
                return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception("Error Updating Order Status" + ex.Message);
            }
        }

        public async Task<dynamic> AuditAsync(dynamic paramJson)
        {
            int woId = paramJson!.woId;
            var orderEntity = await workOrderRepository!.GetEntityAsync(expression: src => src.Id == woId);

            if (orderEntity == null)
                return new { success = false, message = "未知工单" };

            int quantity = paramJson.nonStdCount is null ? 0 : paramJson.nonStdCount;
            int[] stdIds = JsonConvert.DeserializeObject<int[]>(paramJson.stdIds.ToString());

            if ((quantity + stdIds.Length) > 3)
                return new { success = false, message = "砂轮组数量超出限制" };

            await fgmsDbContext!.BeginTrans();
            try
            {
                //标准组
                if (stdIds.Length > 0)
                {
                    foreach (var id in stdIds)
                    {
                        workOrderStandardRepository!.AddEntity(new WorkOrderStandard { WorkOrderId = woId, StandardId = id });
                    }
                }
                //非标组
                if (quantity > 0)
                {
                    var cs = await cargoSpaceRepositroy!.GetEntityAsync(expression: src => src.Code!.Equals("NSG"));

                    if (cs == null)
                        return new { success = false, message = "未提供非标组货位，请联系管理员" };

                    for (int i = 0; i < quantity; i++)
                    {
                        componentRepository!.AddEntity(new Component
                        {
                            IsStandard = false,
                            WorkOrderId = woId,
                            CargoSpaceHistory = cs.Id,
                            Status = ElementEntityStatus.出库
                        });
                    }
                }

                orderEntity.Status = WorkOrderStatus.审核通过;
                workOrderRepository.UpdateEntity(orderEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                bool success = await fgmsDbContext.SaveChangesAsync() > 0;
                await fgmsDbContext.CommitTrans();
                return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
            }
            catch (Exception ex)
            {
                await fgmsDbContext.RollBackTrans();
                throw new Exception("Error Updating Order Status" + ex.Message);
            }
        }
    }
}
