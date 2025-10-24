using System.Linq.Expressions;
using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FGMS.Android.Api.Controllers
{
    /// <summary>
    /// 工单接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/android/[controller]/[action]")]
    public class WorkOrderController : ControllerBase
    {
        private readonly IEquipmentService equipmentService;
        private readonly IWorkOrderService workOrderService;
        private readonly IComponentService componentService;
        private readonly IElementEntityService elementEntityService;
        private readonly ITrackLogService trackLogService;
        private readonly IComponentLogService componentLogService;
        private readonly UserOnline userOnline;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipmentService"></param>
        /// <param name="workOrderService"></param>
        /// <param name="componentService"></param>
        /// <param name="elementEntityService"></param>
        /// <param name="trackLogService"></param>
        /// <param name="componentLogService"></param>
        /// <param name="userOnline"></param>
        /// <param name="mapper"></param>
        public WorkOrderController(
            IEquipmentService equipmentService,
            IWorkOrderService workOrderService,
            IComponentService componentService,
            IElementEntityService elementEntityService,
            ITrackLogService trackLogService,
            IComponentLogService componentLogService,
            UserOnline userOnline,
            IMapper mapper)
        {
            this.equipmentService = equipmentService;
            this.workOrderService = workOrderService;
            this.componentService = componentService;
            this.elementEntityService = elementEntityService;
            this.trackLogService = trackLogService;
            this.componentLogService = componentLogService;
            this.userOnline = userOnline;
            this.mapper = mapper;
        }

        /// <summary>
        /// 获取设备工单
        /// </summary>
        /// <param name="equipmentCode">设备编码</param>
        /// <param name="type">工单类型</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<dynamic> ListAsync(string? equipmentCode, string? type)
        {
            var expression = ExpressionBuilder.GetTrue<WorkOrder>().And(
                src => src.Status != WorkOrderStatus.待审 && src.Status != WorkOrderStatus.驳回 && 
                src.Status != WorkOrderStatus.机台接收 && src.Status != WorkOrderStatus.工单配送 && 
                src.Status != WorkOrderStatus.工单结束 && src.Status != WorkOrderStatus.取消);

            if (!string.IsNullOrEmpty(equipmentCode))
                expression = expression.And(src => src.Equipment!.Code.Equals(equipmentCode));
            if (!string.IsNullOrEmpty(type))
                expression = expression.And(src => src.Type == (WorkOrderType)Enum.Parse(typeof(WorkOrderType), type));

            var entities = await workOrderService.ListAsync(
                expression,
                include: src => src.Include(src => src.Equipment!)
                    .Include(src => src.Equipment!).ThenInclude(src => src.Organize!)
                    .Include(src => src.UserInfo!)
                    .Include(src => src.WorkOrderStandards!).ThenInclude(src => src.Standard!)
                    .Include(src => src.Components!)
                    .ThenInclude(src => src.ElementEntities!.OrderBy(src => src.Position)).ThenInclude(src => src.Element!));
            return mapper.Map<List<WorkOrderDto>>(entities.OrderByDescending(src => src.CreateDate).ToList());
        }

        /// <summary>
        /// 按工件编码查找工单
        /// </summary>
        /// <param name="eeCode">砂轮编码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<dynamic> SearchAsync(string eeCode)
        {
            var entity = await workOrderService.ModelAsync(
                expression: src => src.Components!.FirstOrDefault(src => src.ElementEntities!.FirstOrDefault(src => src.Code!.Equals(eeCode)) != null) != null,
                include: src => src.Include(src => src.Equipment).Include(src => src.UserInfo).Include(src => src.Components!).ThenInclude(src => src.ElementEntities!).ThenInclude(src => src.Element!));

            if (entity == null)
                return new { success = false, message = "工单不存在或已删除" };

            foreach (var cmp in entity.Components!)
            {
                cmp.ElementEntities = cmp.ElementEntities!.OrderBy(src => src.Position).AsEnumerable();
            }
            return mapper.Map<WorkOrderDto>(entity);
        }

        /// <summary>
        /// 机台接收
        /// </summary>
        /// <param name="paramJson">{ 'id': int }</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<dynamic> ReceiveAsync([FromBody] dynamic paramJson)
        {
            if (paramJson == null || paramJson!.id == null)
                return new { success = false, message = "参数错误" };
            bool success = false;
            int orderId = paramJson!.id;
            var orderEntity = await workOrderService.ModelAsync(
                expression: src => src.Id == orderId, include: src => src.Include(src => src.Parent!).Include(src => src.Components!).ThenInclude(src => src.ElementEntities!).Include(src => src.Equipment!));

            if (orderEntity.Type == WorkOrderType.砂轮返修 && orderEntity.Parent != null)
            {
                var parent = orderEntity.Parent;
                var cmps = orderEntity.Components!.ToList();
                cmps.ForEach(cmp =>
                {
                    cmp.WorkOrder = null;
                    cmp.WorkOrderId = parent.Id;
                });
                success = await componentService.UpdateAsync(cmps, new Expression<Func<Component, object>>[] { src => src.WorkOrderId! });
                if (success)
                {
                    orderEntity.Components = null;
                    success = await workOrderService.RemoveAsync(orderEntity);
                }
            }
            else if (orderEntity.Type == WorkOrderType.砂轮申领)
            {
                var cmps = orderEntity.Components;
                orderEntity.Components = null;
                //orderEntity.Equipment = null;
                orderEntity.Status = WorkOrderStatus.机台接收;
                success = await workOrderService.UpdateAsync(orderEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
                if (success)
                {
                    var logs = new List<TrackLog>();
                    cmps!.ToList().ForEach(cmp =>
                    {
                        cmp.ElementEntities!.ToList().ForEach(ee => logs.Add(new TrackLog
                        {
                            Type = LogType.其他,
                            Content = $"工件：{ee.MaterialNo} | 机台：{orderEntity.Equipment!.Code} 接收"
                        }));
                    });
                    await trackLogService.AddAsync(logs);
                }
            }
            return new { success, message = success ? "工单已接收" : "接收失败" };
        }

        /// <summary>
        /// 砂轮房接收
        /// </summary>
        /// <param name="paramJson">{ 'woId': int }</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<dynamic> OrderReceiveAsync([FromBody] dynamic paramJson)
        {
            if (paramJson == null || paramJson!.woId == null)
                return new { success = false, message = "参数错误" };
            return await workOrderService.ReceiveAsync(paramJson);
        }

        /// <summary>
        /// 创建返修单
        /// </summary>
        /// <param name="paramJson">{ 'workOrderId': int, 'equipmentId': int, 'materialNo': 'string', 'materialSpec': 'string', 'ees': [ { 'id': int, 'remark': 'string' } ] }</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<dynamic> RepairAsync([FromBody] dynamic paramJson)
        {
            int workOrderId = paramJson.workOrderId;
            var workOrder = await workOrderService.ModelAsync(
                expression: src => src.Id == workOrderId, 
                include: src => src.Include(src => src.Equipment!).ThenInclude(src => src.Organize!).Include(src => src.Components!).ThenInclude(src => src.ElementEntities!));

            if (workOrder.Components!.Any(src => src.ElementEntities!.FirstOrDefault(src => src.Status != ElementEntityStatus.出库 && src.Status != ElementEntityStatus.下机) != null))
                return new { success = false, message = "工件状态为出库或下机，才能创建返修单" };

            //创建报损工单
            string orderNum = $"RO{GenerateOrderNumber()}";
            var bxOrder = new WorkOrder
            {
                Pid = workOrderId,
                EquipmentId = paramJson.equipmentId,
                UserInfoId = userOnline.Id!.Value,
                OrderNo = orderNum,
                Priority = WorkOrderPriority.高,
                Type = WorkOrderType.砂轮返修,
                MaterialNo = paramJson.materialNo,
                MaterialSpec = paramJson.materialSpec,
                Status = WorkOrderStatus.呼叫AGV,
                AgvTaskCode = Guid.NewGuid().ToString("N")[..16]
            };
            var success = await workOrderService.AddAsync(bxOrder);

            if (!success) return new { success, message = "返修单创建失败" };

            //更新返修工件状态
            var cmps = new List<Component>();
            List<dynamic> eeList = JsonConvert.DeserializeObject<List<dynamic>>(paramJson.ees.ToString());
            int[] eeids = eeList.Select(src => (int)src.id).ToArray();
            var ees = workOrder.Components!.SelectMany(src => src.ElementEntities!).Where(src => eeids.Contains(src.Id)).ToList();
            foreach (var ee in ees)
            {
                ee.Status = ElementEntityStatus.返修;
                ee.Remark = eeList.FirstOrDefault(src => src.id == ee.Id)!.remark;
                if (!cmps.Any(src => src.Id == ee.ComponentId!.Value))
                    cmps.Add(ee.Component!);
                ee.Component = null;
            }
            success = await elementEntityService.UpdateAsync(ees!, new Expression<Func<ElementEntity, object>>[] { src => src.Status, src => src.Remark! });

            //工件组绑定返修单
            cmps.ForEach(x =>
            {
                x.WorkOrderId = bxOrder.Id;
                x.ElementEntities = null;
                x.WorkOrder = null;
            });
            success = await componentService.UpdateAsync(cmps, new Expression<Func<Component, object>>[] { src => src.WorkOrderId! });

            //添加日志
            if (success)
            {
                var logs = new List<TrackLog>();
                ees.ForEach(ee => logs.Add(new TrackLog { Type = LogType.返修, Content = $"机台：{paramJson.EquipmentCode}，工件：{ee.MaterialNo}返修" }));
                await trackLogService.AddAsync(logs);
            }
            //呼叫agv
            return new { success, message = success ? $"已创建返修单：{orderNum}" : "返修失败", data = success ? new { agvTaskCode = bxOrder.AgvTaskCode, start = workOrder.Equipment!.Organize!.Code } : null };
        }

        /// <summary>
        /// 返修配送
        /// </summary>
        /// <param name="paramJson">{ 'workOrderNo': 'string' }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPut]
        public async Task<dynamic> ReturnedSend([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.workOrderNo is null)
                throw new ArgumentNullException(nameof(paramJson));
            string workOrderNo = paramJson.workOrderNo;
            var wo = await workOrderService.ModelAsync(expression: src => src.OrderNo.Equals(workOrderNo));
            if (wo is null)
                return new { success = false, message = "工单不存在或已删除" };
            wo.Status = WorkOrderStatus.返修配送;
            bool success = await workOrderService.UpdateAsync(wo, new Expression<Func<WorkOrder, object>>[] { src => src.Status, src => src.AgvTaskCode });
            return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
        }

        /// <summary>
        /// 整备
        /// </summary>
        /// <param name="paramJson">{ 'woId': int, 'stdCmps': [ { 'cmpId': int, ees:[ { 'id': int, 'position': 'string' } ] } ], 'nonStdCmps': [ { 'cmpId': int, 'eeIds': [{ 'id': int, position: 'string' }] } ] }</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<dynamic> ReadyAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.woId is null)
                throw new ArgumentNullException(nameof(paramJson));
            return await workOrderService.ReadyActionAsync(paramJson);
        }

        /// <summary>
        /// 修整
        /// </summary>
        /// <param name="dto">JSON</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<dynamic> RenovatedAsync([FromBody] ElementEntityDto dto)
        {
            var entity = mapper.Map<ElementEntity>(dto);
            if (entity.Status != ElementEntityStatus.出库)
                entity.Status = ElementEntityStatus.出库;
            bool success = await elementEntityService.UpdateAsync(entity, new Expression<Func<ElementEntity, object>>[]
            {
                src => src.Status,
                src => src.BigDiameter!,
                src => src.SmallDiameter!,
                src => src.InnerDiameter!,
                src => src.OuterDiameter!,
                src => src.Width!,
                src => src.BigRangle!,
                src => src.SmallRangle!,
                src => src.PlaneWidth!,
                src => src.AxialRunout!,
                src => src.RadialRunout!,
                src => src.CurrentAngle!
            });
            if (success)
                await trackLogService.AddAsync(new TrackLog { Type = LogType.整修, Content = $"工件：{entity.MaterialNo} 整修" });
            return new { success, message = success ? "修整成功" : "修整失败" };
        }

        /// <summary>
        /// 砂轮组更换砂轮
        /// </summary>
        /// <param name="paramJson">{ 'cmpId': int, 'oldEeId': int, 'newEeCode': 'string' }</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<dynamic> ReplaceAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.cmpId is null || paramJson.oldEeId is null || paramJson.newEeCode is null)
                throw new ArgumentNullException(paramJson);

            int cmpId = paramJson.cmpId;
            int oldId = paramJson.oldEeId;
            string newCode = paramJson.newEeCode;

            var oldEe = await elementEntityService.ModelAsync(expression: src => src.Id == oldId, include: src => src.Include(src => src.Component!));
            if (oldEe is null) return new { success = false, message = "未知工件" };

            ElementEntity newEe;
            if (oldEe.Component!.IsStandard)
            {
                var ees = await elementEntityService.ListAsync(expression: src => src.ElementId == oldEe.ElementId);
                var codes = ees.Where(src => src.Status == ElementEntityStatus.在库).Select(src => src.Code);
                if (!codes.Contains(newCode))
                    return new { success = false, message = $"标准砂轮组 {oldEe.Component.Code} 未包含此工件" };
                newEe = ees.FirstOrDefault(src => src.Code!.Equals(newCode))!;
            }
            else
            {
                newEe = await elementEntityService.ModelAsync(expression: src => src.Code!.Equals(newCode));
                if (newEe is null)
                    return new { success = false, message = "未知工件" };
            }

            if (newEe.Status != ElementEntityStatus.在库)
                return new { success = false, message = "工件状态异常" };

            var updateList = new List<ElementEntity>();

            oldEe.Component = null;
            oldEe.ComponentId = new int?();
            oldEe.CargoSpaceId = oldEe.CargoSpaceHistory!.Value;
            oldEe.IsGroup = false;
            oldEe.Status = ElementEntityStatus.在库;
            updateList.Add(oldEe);

            newEe.ComponentId = cmpId;
            newEe.CargoSpace = null;
            newEe.CargoSpaceId = new int?();
            newEe.IsGroup = true;
            newEe.Status = ElementEntityStatus.返修;
            updateList.Add(newEe);

            bool success = await elementEntityService.UpdateAsync(updateList, new Expression<Func<ElementEntity, object>>[]
            {
                src => src.ComponentId,
                src => src.CargoSpaceId,
                src => src.IsGroup,
                src => src.Status
            });
            return new { success, message = success ? "操作成功" : "操作失败" };
        }

        /// <summary>
        /// 整备完成
        /// </summary>
        /// <param name="paramJson">{ 'woId': int }</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<dynamic> ReadyCompleteAsync([FromBody] dynamic paramJson)
        {
            if (paramJson.woId is null)
                return new { success = false, message = "参数错误" };
            int woId = paramJson.woId;
            var orderEntity = await workOrderService.ModelAsync(expression: src => src.Id == woId);
            if (orderEntity is null)
                return new { success = false, message = "工单不存在" };
            orderEntity.Status = WorkOrderStatus.整备完成;
            bool success = await workOrderService.UpdateAsync(orderEntity, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
            return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
        }

        /// <summary>
        /// 退仓
        /// </summary>
        /// <param name="paramJson">{ 'workOrderId': int, 'remark': 'string' }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPut]
        public async Task<dynamic> RefundedAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.workOrderId is null)
                throw new ArgumentNullException(nameof(paramJson));
            int woId = paramJson.workOrderId;
            var entity = await workOrderService.ModelAsync(
                expression: src => src.Id == woId,
                include: src => src.Include(src => src.Equipment!).ThenInclude(src => src.Organize).Include(src => src.Components!).ThenInclude(src => src.ElementEntities!).Include(src => src.Childrens!));
            var ees = entity.Components!.SelectMany(src => src.ElementEntities!);

            if (entity.Childrens!.Any())
                return new { success = false, message = "已执行返修流程，无法退仓" };
            if (!ees.Any())
                return new { success = false, message = "工单异常" };
            if (ees.FirstOrDefault(src => src.Status != ElementEntityStatus.下机 && src.Status != ElementEntityStatus.出库) != null)
                return new { success = false, message = "工件状态异常！无法退仓" };

            entity.Type = WorkOrderType.砂轮退仓;
            entity.Status = WorkOrderStatus.AGV收料;
            entity.AgvTaskCode = Guid.NewGuid().ToString("N")[..16];
            var updateExp = new List<Expression<Func<WorkOrder, object>>>
            {
                src => src.Type,
                src => src.Status,
                src => src.AgvTaskCode
            };
            if (paramJson.remark is not null)
            {
                entity.Remark = paramJson.remark;
                updateExp.Add(src => src.Remark!);
            }
            bool success = await workOrderService.UpdateAsync(entity, updateExp.ToArray());
            return new { success, message = success ? "操作成功" : "操作失败", data = success ? new { agvTaskCode = entity.AgvTaskCode, start = entity.Equipment!.Organize!.Code } : null };
        }

        /// <summary>
        /// 上机
        /// </summary>
        /// <param name="paramJson">{'workOrderId': int}</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPut]
        public async Task<dynamic> UpperAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.workOrderId is null)
                throw new ArgumentNullException(nameof(paramJson));

            int woId = paramJson.workOrderId;
            var order = await workOrderService.ModelAsync(
                expression: src => src.Id == woId, 
                include: src => src.Include(src => src.Childrens).Include(src => src.Equipment).Include(src => src.Components!).ThenInclude(src => src.ElementEntities!));

            if (order == null)
                return new { success = false, message = "未知工单" };

            if (order.Childrens is not null && order.Childrens.Any())
                return new { success = false, message = $"{order.OrderNo}包含返修单，请待返修流程结束" };

            var equipment = order.Equipment;

            if (equipment is null)
                return new { success = false, message = "未知设备" };

            if (equipment.Mount)
                return new { success = false, message = "该设备已挂载砂轮组，请勿重复上机" };

            var logs = new List<TrackLog>();
            var ees = order.Components!.SelectMany(src => src.ElementEntities!).ToList();
            ees.ForEach(ee =>
            {
                ee.Status = ElementEntityStatus.上机;
                ee.BeginTime = DateTime.Now;
                ee.Remark = null;
                ee.Component = null;
                logs.Add(new TrackLog
                {
                    Content = $"工件：{ee.MaterialNo}已上机，机台：{order.Equipment!.Code}，时间：{ee.BeginTime.Value}",
                    JsonContent = JsonConvert.SerializeObject(new
                    {
                        ee.Code,
                        ee.BigDiameter,
                        ee.SmallDiameter,
                        ee.InnerDiameter,
                        ee.OuterDiameter,
                        ee.Width,
                        ee.BigRangle,
                        ee.SmallRangle,
                        ee.PlaneWidth,
                        ee.AxialRunout,
                        ee.RadialRunout,
                        ee.CurrentAngle,
                        ee.BeginTime,
                        ee.FinishTime,
                        ee.UseDuration
                    })
                });
            });
            equipment.Mount = true;
            bool success =
                await elementEntityService.UpdateAsync(ees, new Expression<Func<ElementEntity, object>>[] { src => src.Status, src => src.BeginTime, src => src.Remark }) &&
                await equipmentService.UpdateAsync(equipment, new Expression<Func<Equipment, object>>[] { src => src.Mount });

            if (success)
            {
                var components = order.Components!.Where(src => src.IsStandard == true) ?? null;
                if (components != null && components.Any())
                {
                    var addList = new List<ComponentLog>();
                    foreach (var cmp in components)
                    {
                        string equipmentCode = cmp.WorkOrder!.Equipment!.Code;
                        string orderNo = cmp.WorkOrder.OrderNo;
                        var upperEes = cmp.ElementEntities!.Select(ee => new
                        {
                            ee.Code,
                            ee.BigDiameter,
                            ee.SmallDiameter,
                            ee.InnerDiameter,
                            ee.OuterDiameter,
                            ee.Width,
                            ee.BigRangle,
                            ee.SmallRangle,
                            ee.PlaneWidth,
                            ee.AxialRunout,
                            ee.RadialRunout,
                            ee.CurrentAngle,
                            ee.BeginTime,
                            ee.FinishTime,
                            ee.UseDuration,
                            Status = "上机"
                        });
                        string json = JsonConvert.SerializeObject(upperEes);
                        addList.Add(new ComponentLog
                        {
                            Code = cmp.Code!,
                            OrderNo = orderNo,
                            MaterialNo = order.MaterialNo,
                            MaterialSpec = order.MaterialSpec,
                            EquipmentCode = equipmentCode,
                            RequiredDate = order.RequiredDate!.Value,
                            UpperJson = json
                        });
                    }
                    await componentLogService.AddAsync(addList);
                }
                await trackLogService.AddAsync(logs);
            }
            return new { success, message = success ? "上机完成" : "上机失败" };
        }

        /// <summary>
        /// 下机
        /// </summary>
        /// <param name="dtos">json arrary</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<dynamic> DownAsync([FromBody] List<ComponentDto> dtos)
        {
            if (dtos is null)
                throw new ArgumentNullException(nameof(dtos));
            
            int woId = dtos.FirstOrDefault()!.WorkOrderId!.Value;
            var orderEntity = await workOrderService.ModelAsync(expression: src => src.Id == woId, include: src => src.Include(src => src.Equipment!));

            if (orderEntity is null)
                return new { success = false, message = "未知工单" };

            var cmps = mapper.Map<List<Component>>(dtos);
            var updateEeList = new List<ElementEntity>();
            var logs = new List<TrackLog>();
            var finishTime = DateTime.Now;
            cmps.ForEach(cmp =>
            {
                cmp.ElementEntities!.ToList().ForEach(ee =>
                {
                    ee.Status = ElementEntityStatus.下机;
                    ee.FinishTime = finishTime;
                    ee.UseDuration += (float)(finishTime - ee.BeginTime!.Value).TotalHours;
                    updateEeList.Add(ee);
                    //单片砂轮日志
                    logs.Add(new TrackLog
                    {
                        Content = $"工件：{ee.MaterialNo}已下机，时间：{ee.FinishTime.Value}",
                        JsonContent = JsonConvert.SerializeObject(new
                        {
                            ee.Code,
                            ee.BigDiameter,
                            ee.SmallDiameter,
                            ee.InnerDiameter,
                            ee.OuterDiameter,
                            ee.Width,
                            ee.BigRangle,
                            ee.SmallRangle,
                            ee.PlaneWidth,
                            ee.AxialRunout,
                            ee.RadialRunout,
                            ee.CurrentAngle,
                            ee.BeginTime,
                            ee.FinishTime,
                            ee.UseDuration
                        })
                    });
                });
            });

            var equipment = orderEntity.Equipment;
            equipment!.Mount = false;

            bool success = await elementEntityService.UpdateAsync(updateEeList, new Expression<Func<ElementEntity, object>>[]
            {
                src => src.Status,
                src => src.BigDiameter,
                src => src.SmallDiameter,
                src => src.InnerDiameter,
                src => src.OuterDiameter,
                src => src.Width,
                src => src.BigRangle,
                src => src.SmallRangle,
                src => src.PlaneWidth,
                src => src.AxialRunout,
                src => src.RadialRunout,
                src => src.CurrentAngle,
                src => src.FinishTime,
                src => src.UseDuration
            }) && await equipmentService.UpdateAsync(equipment, new Expression<Func<Equipment, object>>[] { src => src.Mount });

            if (success)
            {
                var updateCmpLogs = new List<ComponentLog>();
                foreach (var cmp in cmps)
                {
                    if (cmp.IsStandard)
                    {
                        var cmpLog = componentLogService.ModelAsync(expression: src => src.OrderNo.Equals(orderEntity.OrderNo) && src.DownJson == null).Result;
                        if (cmpLog != null)
                        {
                            var downEes = cmp.ElementEntities!.Select(ee => new
                            {
                                ee.Code,
                                ee.BigDiameter,
                                ee.SmallDiameter,
                                ee.InnerDiameter,
                                ee.OuterDiameter,
                                ee.Width,
                                ee.BigRangle,
                                ee.SmallRangle,
                                ee.PlaneWidth,
                                ee.AxialRunout,
                                ee.RadialRunout,
                                ee.CurrentAngle,
                                ee.BeginTime,
                                FinishTime = finishTime,
                                ee.UseDuration,
                                Status = "下机"
                            });
                            cmpLog.DownJson = JsonConvert.SerializeObject(downEes);
                            updateCmpLogs.Add(cmpLog);
                        }
                    }
                }
                await componentLogService.UpdateAsync(updateCmpLogs, new Expression<Func<ComponentLog, object>>[] { src => src.DownJson });
                await trackLogService.AddAsync(logs);
            }

            return new { success, message = success ? "下机完成" : "下机失败" };
        }

        //[HttpPost]
        //public async Task<dynamic> OffAsync([FromBody] List<ComponentDto> dtos)
        //{
        //    if (dtos is null)
        //        throw new ArgumentNullException(nameof(dtos));

        //    int woId = dtos.FirstOrDefault()!.WorkOrderId!.Value;
        //    var orderEntity = await workOrderService.ModelAsync(expression: src => src.Id == woId, include: src => src.Include(src => src.Equipment!));

        //    if (orderEntity is null)
        //        return new { success = false, message = "未知工单" };

        //    var cmps = mapper.Map<List<Component>>(dtos);
        //    var updateEeList = new List<ElementEntity>();
        //    var logs = new List<TrackLog>();
        //    var updateCmpLogs = new List<ComponentLog>();
        //    var offDate = DateTime.Now;
        //    cmps.ForEach(cmp =>
        //    {
        //        cmp.ElementEntities!.ToList().ForEach(ee =>
        //        {
        //            ee.Status = ElementEntityStatus.下机;
        //            ee.FinishTime = offDate;
        //            ee.UseDuration += (float)(ee.FinishTime.Value - ee.BeginTime!.Value).TotalHours;
        //            updateEeList.Add(ee);
        //            //单片砂轮日志
        //            logs.Add(new TrackLog
        //            {
        //                Content = $"工件：{ee.MaterialNo}已下机，时间：{ee.FinishTime.Value}",
        //                JsonContent = JsonConvert.SerializeObject(new
        //                {
        //                    ee.Code,
        //                    ee.BigDiameter,
        //                    ee.SmallDiameter,
        //                    ee.InnerDiameter,
        //                    ee.OuterDiameter,
        //                    ee.Width,
        //                    ee.BigRangle,
        //                    ee.SmallRangle,
        //                    ee.PlaneWidth,
        //                    ee.AxialRunout,
        //                    ee.RadialRunout,
        //                    ee.CurrentAngle,
        //                    ee.BeginTime,
        //                    ee.FinishTime,
        //                    ee.UseDuration
        //                })
        //            });
        //        });

        //        if (cmp.IsStandard)
        //        {
        //            var cmpLog = componentLogService.ModelAsync(expression: src => src.OrderNo.Equals(orderEntity.OrderNo) && src.DownJson == null).Result;
        //            if (cmpLog != null)
        //            {
        //                var downEes = cmp.ElementEntities!.Select(ee => new
        //                {
        //                    ee.Code,
        //                    ee.BigDiameter,
        //                    ee.SmallDiameter,
        //                    ee.InnerDiameter,
        //                    ee.OuterDiameter,
        //                    ee.Width,
        //                    ee.BigRangle,
        //                    ee.SmallRangle,
        //                    ee.PlaneWidth,
        //                    ee.AxialRunout,
        //                    ee.RadialRunout,
        //                    ee.CurrentAngle,
        //                    ee.BeginTime,
        //                    FinishTime = offDate,
        //                    ee.UseDuration,
        //                    Status = "下机"
        //                });
        //                cmpLog.DownJson = JsonConvert.SerializeObject(downEes);
        //                updateCmpLogs.Add(cmpLog);
        //            }
        //        }
        //    });

        //    var equipment = orderEntity.Equipment;
        //    equipment!.Mount = false;

        //    bool success = await elementEntityService.UpdateAsync(updateEeList, new Expression<Func<ElementEntity, object>>[]
        //    {
        //        src => src.Status,
        //        src => src.BigDiameter,
        //        src => src.SmallDiameter,
        //        src => src.InnerDiameter,
        //        src => src.OuterDiameter,
        //        src => src.Width,
        //        src => src.BigRangle,
        //        src => src.SmallRangle,
        //        src => src.PlaneWidth,
        //        src => src.AxialRunout,
        //        src => src.RadialRunout,
        //        src => src.CurrentAngle,
        //        src => src.FinishTime,
        //        src => src.UseDuration
        //    }) && await equipmentService.UpdateAsync(equipment, new Expression<Func<Equipment, object>>[] { src => src.Mount });

        //    if (success)
        //    {
        //        await componentLogService.UpdateAsync(updateCmpLogs, new Expression<Func<ComponentLog, object>>[] { src => src.DownJson });
        //        await trackLogService.AddAsync(logs);
        //    }

        //    return new { success, message = success ? "下机完成" : "下机失败" };
        //}

        /// <summary>
        /// 工单状态更新
        /// </summary>
        /// <param name="paramJson">{ 'woId': int, 'status': 'string' }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPut]
        public async Task<dynamic> StatusUpdate([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.woId is null || paramJson.status is null)
                throw new ArgumentNullException(nameof(paramJson));
            int woId = paramJson.woId;
            string status = paramJson.status;
            var order = await workOrderService.ModelAsync(expression: src => src.Id == woId);
            if (order is null)
                return new { success = false, message = "工单不存在" };
            order.Status = (WorkOrderStatus)Enum.Parse(typeof(WorkOrderStatus), status);
            bool success = await workOrderService.UpdateAsync(order, new Expression<Func<WorkOrder, object>>[] { src => src.Status });
            return new { success, message = success ? "工单状态已更新" : "工单状态更新失败" };
        }

        /// <summary>
        /// 生成订单号
        /// </summary>
        /// <returns></returns>
        private static string GenerateOrderNumber()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            Guid guid = Guid.NewGuid();
            string randomPart = guid.ToString()[..4].ToUpper();
            return $"{datePart}{randomPart}";
        }
    }
}
