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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Newtonsoft.Json;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 组件接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/component")]
    public class ComponentController : ControllerBase
    {
        private readonly IComponentService componentService;
        private readonly IComponentLogService componentLogService;
        private readonly IElementEntityService elementEntityService;
        private readonly GenerateRandomNumber randomNumber;
        private readonly UserOnline userOnline;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentService"></param>
        /// <param name="componentLogService"></param>
        /// <param name="elementEntityService"></param>
        /// <param name="randomNumber"></param>
        /// <param name="userOnline"></param>
        /// <param name="mapper"></param>
        public ComponentController(
            IComponentService componentService, 
            IComponentLogService componentLogService, 
            IElementEntityService elementEntityService, 
            GenerateRandomNumber randomNumber, 
            UserOnline userOnline, 
            IMapper mapper)
        {
            this.componentService = componentService;
            this.componentLogService = componentLogService;
            this.elementEntityService = elementEntityService;
            this.randomNumber = randomNumber;
            this.userOnline = userOnline;
            this.mapper = mapper;
        }

        /// <summary>
        /// 组件集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="cmpCode">标组编码</param>
        /// <param name="isStd">是否标组</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<dynamic> ListAsync(int? pageIndex,int? pageSize, string? cmpCode, bool isStd = false)
        {
            var expression = ExpressionBuilder.GetTrue<Component>().And(src => src.IsStandard == isStd);

            //链式添加include
            IIncludableQueryable<Component, object> include(IQueryable<Component> src)
            {
                IIncludableQueryable<Component, object> query = src.Include(src => src.CargoSpace).Include(x => x.ElementEntities!.OrderBy(x => x.Element!.Category)).ThenInclude(x => x.Element).Include(x => x.WorkOrder!);
                if (isStd)
                    query = query.Include(x => x.Standard!);
                return query;
            }

            if (!string.IsNullOrEmpty(cmpCode))
                expression = expression.And(src => src.Standard!.Code.Contains(cmpCode));

            var cmps = await componentService.ListAsync(expression, include);
            int total = cmps.Count;
            if (pageIndex.HasValue && pageSize.HasValue)
                cmps = cmps.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            return new { total, rows = mapper.Map<List<ComponentDto>>(cmps) };
        }

        /// <summary>
        /// 标准组历史数据
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="cmpCode">标准编码</param>
        /// <param name="workOrderNo">工单号</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns></returns>
        [HttpGet("standardHistoryList")]
        public async Task<dynamic> StandardHistoryList(int? pageIndex, int? pageSize, string? cmpCode, string? workOrderNo, DateTime? startDate, DateTime? endDate)
        {
            var expression = ExpressionBuilder.GetTrue<ComponentLog>();

            if (!string.IsNullOrEmpty(cmpCode))
                expression = expression.And(src => src.Code.Contains(cmpCode));

            if (!string.IsNullOrEmpty(workOrderNo))
                expression = expression.And(src => src.OrderNo.Contains(workOrderNo));

            if (startDate.HasValue && endDate.HasValue)
                expression = expression.And(src => src.RequiredDate >= startDate.Value && src.RequiredDate <= endDate.Value.AddHours(24));

            var records = await componentLogService.ListAsync(expression);
            int total = records.Count;

            if (pageIndex.HasValue && pageSize.HasValue)
                records = records.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();

            return new { total, rows = mapper.Map<List<ComponentLogDto>>(records) };
        }

        /// <summary>
        /// 工件编码查找相关标准组件
        /// </summary>
        /// <param name="eeCode">工件编码</param>
        /// <param name="status">状态</param>
        /// <returns></returns>
        [HttpGet("single")]
        public async Task<dynamic> SingleAsync(string eeCode, string? status)
        {
            var expression = ExpressionBuilder.GetTrue<Component>()
                .And(src => src.ElementEntities!.FirstOrDefault(src => src.Code!.Equals(eeCode)) != null)
                .AndIf(!string.IsNullOrEmpty(status), src => src.Status == Enum.Parse<ElementEntityStatus>(status!));
            var component = await componentService.ModelAsync(expression, include:src => src.Include(src => src.ElementEntities!).ThenInclude(src => src.Element!).Include(src => src.Standard!));
            if (component == null)
                return new { success = false, message = "未找到相关标准组，或该标准组已出库" };
            return mapper.Map<ComponentDto>(component);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="paramJson">{ 'isstd': bool, 'stdid': int, 'stdcode': 'string', 'csid': int, 'woid': int, eeids: [int] }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPost("add")]
        public async Task<dynamic> AddAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.isstd is null || paramJson.eeids is null)
                throw new ArgumentNullException(nameof(paramJson));

            bool isstd = paramJson.isstd;
            int[] eeids = JsonConvert.DeserializeObject<int[]>(paramJson.eeids.ToString());
            var ees = await elementEntityService.ListAsync(expression: src => eeids.Contains(src.Id));

            foreach (var ee in ees)
            {
                ee.IsGroup = true;
                if (isstd)
                {
                    if (paramJson.csid is null)
                        return new { success = false, message = "标准组件未提供货位信息" };
                    ee.CargoSpaceId = int.Parse(paramJson.csid.ToString());
                }
                else
                    ee.CargoSpaceId = new int?();
            }

            bool success = await elementEntityService.UpdateAsync(ees, new Expression<Func<ElementEntity, object>>[] { src => src.CargoSpaceId!, src => src.IsGroup });

            if (!success)
                return new { success, message = "货位、组信息更新失败" };

            var component = new Component { IsStandard = isstd, ElementEntities = ees };
            if (component.IsStandard)
            {
                //标组
                if (paramJson.stdid is null || paramJson.stdcode is null)
                    throw new ArgumentNullException(nameof(paramJson));
                component.StandardId = int.Parse(paramJson.stdid.ToString());
                component.CargoSpaceId = int.Parse(paramJson.csid.ToString());
                component.CargoSpaceHistory = component.CargoSpaceId;
                component.Code = $"{paramJson.stdcode}-{randomNumber.Create()}";
                component.Status = ElementEntityStatus.在库;
                component.IsStandard = true;
            }
            else
            {
                //非标
                if (paramJson.woid is null)
                    throw new ArgumentNullException(nameof(paramJson));
                component.WorkOrderId = int.Parse(paramJson.woid.ToString());
            }
            success = await componentService.CombinedAsync(component);
            return new { success, message = success ? "添加成功" : "添加失败", cmpid = success ? component.Id.ToString() : string.Empty };
        }

        /// <summary>
        /// 拆分入库
        /// </summary>
        /// <param name="paramJson">{ 'componentId' : int }</param>
        /// <returns></returns>
        [HttpPut("split")]
        public async Task<dynamic> SplitAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson.componentId is null)
                return new { success = false, message = "参数错误" };
            int componentId = paramJson.componentId;
            return await componentService.SplitAsync(componentId);
        }
    }
}
