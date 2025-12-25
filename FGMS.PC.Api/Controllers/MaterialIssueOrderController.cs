using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.PC.Api.Filters;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using Newtonsoft.Json;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 发料单接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/materialIssueOrder")]
    [PermissionAsync("material_issue_order_management", "management", "电脑")]
    public class MaterialIssueOrderController : ControllerBase
    {
        private readonly IMaterialIssueOrderService materialIssueOrderService;
        private readonly UserOnline userOnline;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="materialIssueOrderService"></param>
        /// <param name="userOnline"></param>
        /// <param name="mapper"></param>
        public MaterialIssueOrderController(IMaterialIssueOrderService materialIssueOrderService, UserOnline userOnline, IMapper mapper)
        {
            this.materialIssueOrderService = materialIssueOrderService;
            this.userOnline = userOnline;
            this.mapper = mapper;
        }

        /// <summary>
        /// 获取发料单列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="keyword">关键字</param>
        /// <param name="type">类型</param>
        /// <param name="status">状态</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<IActionResult> ListAsync(int? pageIndex, int? pageSize, string? keyword, string? type, string? status)
        {
            var expression = ExpressionBuilder.GetTrue<MaterialIssueOrder>()
                .AndIf(!string.IsNullOrEmpty(keyword), src =>
                    src.MaterialNo.Contains(keyword!) ||
                    src.ProductionOrder!.OrderNo.Contains(keyword!) ||
                    src.ProductionOrder!.Equipment!.Code.Contains(keyword!) ||
                    src.ProductionOrder!.Equipment!.Organize!.Code.Contains(keyword!))
                .AndIf(!string.IsNullOrEmpty(type), src => src.Type == Enum.Parse<MioType>(type!))
                .AndIf(!string.IsNullOrEmpty(status), src => src.Status == Enum.Parse<MioStatus>(status!));

            var query = materialIssueOrderService.GetQueryable(
                expression, 
                include: src => src.Include(src => src.ProductionOrder!).ThenInclude(src => src.Equipment!).ThenInclude(src => src.Organize!).Include(src => src.Sendor!).Include(src => src.Createor!))
                .OrderByDescending(x => x.Id)
                .AsNoTracking();

            int total = await query.CountAsync();
            if(pageIndex.HasValue && pageSize.HasValue)
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            var entities = await query.ToListAsync();
            var dtos = mapper.Map<List<MaterialIssueOrderDto>>(entities);
            return Ok(new { total, rows = dtos });
        }

        /// <summary>
        /// 备料
        /// </summary>
        /// <param name="paramJson">{ 'ids':[int] }</param>
        /// <returns></returns>
        [HttpPut("prepare")]
        public async Task<dynamic> PrepareAsync([FromBody] dynamic paramJson)
        {
            if(paramJson is null || paramJson.ids is null)
                return BadRequest(new { success = false, message = "参数错误" });

            List<int> ints = JsonConvert.DeserializeObject<List<int>>(Convert.ToString(paramJson.ids));
            return await materialIssueOrderService.PrepareAsync(ints.ToArray());
        }

        /// <summary>
        /// 发料
        /// </summary>
        /// <param name="paramJson">{ 'ids': [int] }</param>
        /// <returns></returns>
        [HttpPut("send")]
        public async Task<dynamic> SendAsync([FromBody] dynamic paramJson)
        {
            if(paramJson is null || paramJson.ids is null)
                return BadRequest(new { success = false, message = "参数错误" });

            //呼叫agv

            int[] ids = JsonConvert.DeserializeObject<int[]>(Convert.ToString(paramJson.ids));
            int userInfoId = userOnline.Id!.Value;
            return await materialIssueOrderService.OutboundAsync(ids, userInfoId);
        }
    }
}
