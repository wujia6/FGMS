using System.Linq.Expressions;
using FGMS.Android.Api.Filters;
using FGMS.Models;
using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;

namespace FGMS.Android.Api.Controllers
{
    /// <summary>
    /// 工件接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/android/[controller]/[action]")]
    [PermissionAsync("m_whell_management", "management", "移动")]
    public class ElementEntityController : ControllerBase
    {
        private readonly IElementEntityService elementEntityService;
        private readonly ICargoSpaceService cargoSpaceService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementEntityService"></param>
        /// <param name="cargoSpaceService"></param>
        /// <param name="mapper"></param>
        public ElementEntityController(IElementEntityService elementEntityService, ICargoSpaceService cargoSpaceService, IMapper mapper)
        {
            this.elementEntityService = elementEntityService;
            this.cargoSpaceService = cargoSpaceService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 扫描工件编码
        /// </summary>
        /// <param name="code">编码</param>
        /// <param name="status">状态</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<dynamic> ScanAsync(string code, string? status)
        {
            var expression = ExpressionBuilder.GetTrue<ElementEntity>()
                .And(src => src.Code!.Equals(code))
                .AndIf(!string.IsNullOrEmpty(status), src => src.Status.GetDisplayName() == status);

            var entity = await elementEntityService.ModelAsync(expression, include: src => src.Include(src => src.Element!));

            if (entity == null)
                return new { success = false, message = "未知工件，或该工件已出库" };
            else if (entity.IsGroup && entity.ComponentId.HasValue)
            {
                var ees = await elementEntityService.ListAsync(expression: src => src.ComponentId == entity.ComponentId.Value, include: src => src.Include(src => src.Element!));
                return mapper.Map<List<ElementEntityDto>>(ees);
            }
            else
                return mapper.Map<List<ElementEntityDto>>(new List<ElementEntity> { entity });
        }

        /// <summary>
        /// 扫描单个工件编码
        /// </summary>
        /// <param name="code">编码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<dynamic> ScanSingleAsync(string code)
        {
            var entity = await elementEntityService.ModelAsync(expression: src => src.Code!.Equals(code), include: src => src.Include(src => src.Element!));
            return mapper.Map<ElementEntityDto>(entity);
        }

        /// <summary>
        /// 工件报废
        /// </summary>
        /// <param name="paramJson">{ 'id': int, 'discardBy': 'string' }</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<dynamic> AbandonedAsync([FromBody] dynamic paramJson)
        {
            if (paramJson is null || paramJson!.id is null || paramJson!.discardBy is null)
                return Task.FromResult<dynamic>(new { success = false, message = "参数错误" });

            var cargoSpace = await cargoSpaceService.ModelAsync(expression: src => src.Code!.Equals("DCS"));

            if (cargoSpace is null)
                return Task.FromResult<dynamic>(new { success = false, message = "默认废弃仓位不存在，请联系管理员" });

            int eeid = (int)paramJson!.id;
            var entity = await elementEntityService.ModelAsync(src => src.Id == eeid);

            if (entity is null)
                return new { success = false, message = "工件不存在" };
            else if (entity.Status == ElementEntityStatus.报废)
                return new { success = false, message = "工件已报废，无需重复报废" };

            entity.CargoSpaceId = cargoSpace.Id;
            entity.Status = ElementEntityStatus.报废;
            entity.DiscardBy = (DiscardReason)Enum.Parse(typeof(DiscardReason), (string)paramJson!.discardBy);
            entity.DiscardTime = DateTime.Now;
            var success = await elementEntityService.UpdateAsync(entity, new Expression<Func<ElementEntity, object>>[] 
            { 
                src => src.CargoSpaceId,
                src => src.Status,
                src => src.DiscardBy,
                src => src.DiscardTime
            });
            return success ? new { success = true, message = "工件报废成功" } : new { success = false, message = "工件报废失败" };
        }
    }
}
