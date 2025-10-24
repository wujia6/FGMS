using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 角色接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/role")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleInfoService roleInfoService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleInfoService"></param>
        /// <param name="mapper"></param>
        public RoleController(IRoleInfoService roleInfoService, IMapper mapper)
        {
            this.roleInfoService = roleInfoService;
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
            var result = await roleInfoService.ListAsync(include: src => src.Include(src => src.Organize!));
            int total = result.Count;
            if (pageIndex.HasValue && pageSize.HasValue)
                result = result.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            return new { total, rows = mapper.Map<List<RoleInfoDto>>(result) };
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model">JSON</param>
        /// <returns></returns>
        [HttpPost("save")]
        public async Task<dynamic> SaveAsync([FromBody] RoleInfoDto model)
        {
            var entity = mapper.Map<RoleInfo>(model);
            bool success = entity.Id > 0 ? await roleInfoService.UpdateAsync(entity) : await roleInfoService.AddAsync(entity);
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
            int roleId = param!.id;
            var entity = await roleInfoService.ModelAsync(expression: src => src.Id == roleId);
            if (entity == null) return new { success = false, message = "记录不存在或已删除" };
            bool success = await roleInfoService.RemoveAsync(entity);
            return new { success, message = success ? "删除成功" : "删除失败" };
        }
    }
}
