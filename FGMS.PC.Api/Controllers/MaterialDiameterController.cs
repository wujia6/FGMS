using FGMS.Models.Dtos;
using FGMS.Services.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 物料柄径接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/materialDiameter")]
    public class MaterialDiameterController : ControllerBase
    {
        private readonly IMaterialDiameterService materialDiameterService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="materialDiameterService"></param>
        /// <param name="mapper"></param>
        public MaterialDiameterController(IMaterialDiameterService materialDiameterService, IMapper mapper)
        {
            this.materialDiameterService = materialDiameterService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 按柄径信息获取物料标准工时
        /// </summary>
        /// <param name="diameter">柄径</param>
        /// <returns></returns>
        [HttpGet("find")]
        public async Task<IActionResult> FindAsync(double diameter)
        {
            var entity = await materialDiameterService.ModelAsync(expression: src => src.Diameter == diameter);
            if (entity == null)
                return Ok(new { success = false, message = "未找到对应的物料柄径信息" });
            var dto = mapper.Map<MaterialDiameterDto>(entity);
            return Ok(new { success = true, data = dto });
        }
    }
}
