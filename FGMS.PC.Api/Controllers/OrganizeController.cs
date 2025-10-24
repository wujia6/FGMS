using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 组织接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/organize")]
    public class OrganizeController : ControllerBase
    {
        private readonly IOrganizeService organizeService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="organizeService"></param>
        /// <param name="mapper"></param>
        public OrganizeController(IOrganizeService organizeService, IMapper mapper)
        {
            this.organizeService = organizeService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<dynamic> ListAsync(int? pageIndex, int? pageSize)
        {
            var result = await organizeService.ListAsync(include: src => src.Include(src => src.Parent!));
            int total = result.Count;
            if (pageIndex.HasValue && pageSize.HasValue)
                result = result.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            return new { total, rows = mapper.Map<List<OrganizeDto>>(result) };
        }

        /// <summary>
        /// 单个
        /// </summary>
        /// <param name="organizeId">组织ID</param>
        /// <returns></returns>
        [HttpGet("single")]
        public async Task<dynamic> SingleAsync(int organizeId)
        {
            var entity = await organizeService.ModelAsync(expression: src => src.Id == organizeId, include: src => src.Include(src => src.Parent!).Include(src => src.Childrens!));
            if (entity is null)
                return new { success = false, message = "不存在或已删除" };
            return mapper.Map<OrganizeDto>(entity);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model">JSON</param>
        /// <returns></returns>
        [HttpPost("save")]
        public async Task<dynamic> SaveAsync([FromBody] OrganizeDto model)
        {
            var entity = mapper.Map<Organize>(model);
            entity.Pid = entity.Pid == 0 ? null : entity.Pid;
            var success = entity.Id > 0 ? await organizeService.UpdateAsync(entity) : await organizeService.AddAsync(entity);
            return new { success, message = success ? "保存成功" : "保存失败" };
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="param">{ 'id' : int }</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpDelete("remove")]
        public async Task<dynamic> RemoveAsync([FromBody] dynamic param)
        {
            if (param == null || param!.id is null) throw new ArgumentNullException(nameof(param));
            int organizeId = param!.id;
            var entity = await organizeService.ModelAsync(expression: src => src.Id == organizeId);
            if (entity == null) return new { success = false, message = "记录不存在或已删除" };
            bool success = await organizeService.RemoveAsync(entity);
            return new { success, message = success ? "删除成功" : "删除失败" };
        }
    }
}
