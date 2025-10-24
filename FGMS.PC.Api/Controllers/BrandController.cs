using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 品牌接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/brand")]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService brandService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="brandService"></param>
        /// <param name="mapper"></param>
        public BrandController(IBrandService brandService, IMapper mapper)
        {
            this.brandService = brandService;
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
            var entities = await brandService.ListAsync();
            int total = entities.Count;
            if (pageIndex.HasValue && pageSize.HasValue)
                entities = entities.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            return new { total, rows = mapper.Map<List<BrandDto>>(entities) };
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model">JSON</param>
        /// <returns></returns>
        [HttpPost("save")]
        public async Task<dynamic> SaveAsync([FromBody] BrandDto model)
        {
            var entity = mapper.Map<Brand>(model);
            bool success = entity.Id > 0 ? await brandService.UpdateAsync(entity) : await brandService.AddAsync(entity);
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
            int brandId = param!.id;
            var entity = await brandService.ModelAsync(expression: src => src.Id == brandId);
            if (entity == null) return new { success = false, message = "记录不存在或已删除" };
            bool success = await brandService.RemoveAsync(entity);
            return new { success, message = success ? "删除成功" : "删除失败" };
        }
    }
}
