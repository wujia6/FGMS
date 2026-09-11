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
using MiniExcelLibs;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 加工标准
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/processingStandard")]
    public class ProcessingStandardController : ControllerBase
    {
        private readonly IProcessingStandardService processingStandardService;
        private readonly IUserInfoService userInfoService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processingStandardService"></param>
        /// <param name="userInfoService"></param>
        /// <param name="mapper"></param>
        public ProcessingStandardController(IProcessingStandardService processingStandardService, IUserInfoService userInfoService, IMapper mapper)
        {
            this.processingStandardService = processingStandardService;
            this.userInfoService = userInfoService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 获取集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="status">状态</param>
        /// <param name="personName">处理人</param>
        /// <param name="materialCode">料号</param>
        /// <param name="stdGroup">标准砂轮组</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns></returns>
        [HttpGet("list")]
        [PermissionAsync("processing_standard_management", "view", "电脑")]
        public async Task<IActionResult> ListAsync(int? pageIndex, int? pageSize, string? status, string? personName, string? materialCode, string? stdGroup, DateTime? startDate, DateTime? endDate)
        {
            var expression = ExpressionBuilder.GetTrue<ProcessingStandard>()
                .AndIf(!string.IsNullOrEmpty(status), x => x.ProgramStatus == Enum.Parse<GeneralStatus>(status!))
                .AndIf(!string.IsNullOrEmpty(personName), x => x.BomProcessor!.Name.Contains(personName!) || x.Handler!.Name.Contains(personName!))
                .AndIf(!string.IsNullOrEmpty(materialCode), x => x.MaterialNumber.Contains(materialCode!))
                .AndIf(!string.IsNullOrEmpty(stdGroup), x => x.StandardGrindingWheelSet!.Contains(stdGroup!))
                .AndIf(startDate.HasValue && endDate.HasValue, x => x.OrderDate >= startDate!.Value && x.OrderDate <= endDate!.Value.AddHours(24).AddSeconds(-1));

            var query = processingStandardService.GetQueryable(expression)
                .Include(src => src.BomProcessor)
                .Include(src => src.Handler)
                .OrderByDescending(src => src.Id)
                .AsNoTracking();

            int total = await query.CountAsync();
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            var data = await query.ToListAsync();
            return Ok(new { total, rows = mapper.Map<List<ProcessingStandardDto>>(data) });
        }

        /// <summary>
        /// Excel导入
        /// </summary>
        /// <param name="file">EXCEL文件</param>
        /// <returns></returns>
        [HttpPost("import")]
        [PermissionAsync("processing_standard_management", "management", "电脑")]
        public async Task<IActionResult> ImportFromExcelAsync(IFormFile file)
        {
            // 1. 文件验证
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "请上传文件" });
            }

            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (fileExtension != ".xlsx" && fileExtension != ".xls")
            {
                return BadRequest(new { success = false, message = "只支持Excel文件（.xlsx或.xls）" });
            }

            // 2. 限制文件大小（50MB）
            if (file.Length > 50 * 1024 * 1024)
            {
                return BadRequest(new { success = false, message = "文件大小不能超过50MB" });
            }

            // 3. 读取Excel数据
            var stream = file.OpenReadStream();
            var excelRows = MiniExcel.Query<ProcessingStandardDto>(stream).ToList();

            if (excelRows == null || excelRows.Count == 0)
            { 
                return BadRequest(new { success = false, message = "Excel文件中没有数据" }); 
            }

            // 4. 数据验证和预处理
            var validDtos = new List<ProcessingStandardDto>();
            var errors = new List<string>();

            for (int i = 0; i < excelRows.Count; i++)
            {
                var row = excelRows[i];
                var rowNumber = i + 2; // Excel行号（第1行是表头，所以从第2行开始）

                // 跳过完全空行
                if (IsRowEmpty(row))
                {
                    continue;
                }

                // 必填字段验证
                if (string.IsNullOrWhiteSpace(row.ToolName))
                {
                    errors.Add($"第{rowNumber}行：刀具品名不能为空");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.MaterialNumber))
                {
                    errors.Add($"第{rowNumber}行：料号不能为空");
                    continue;
                }

                // 验证日期字段逻辑
                if (row.OrderDate == DateTime.MinValue || row.OrderDate == default)
                {
                    errors.Add($"第{rowNumber}行：接单日期无效");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.ToolSpecification))
                {
                    errors.Add($"第{rowNumber}行：刀具规格不能为空");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.MaterialNumber))
                {
                    errors.Add($"第{rowNumber}行：料号不能为空");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.DrawingType))
                {
                    errors.Add($"第{rowNumber}行：图纸类型不能为空");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.ToolType))
                {
                    errors.Add($"第{rowNumber}行：刀具类型不能为空");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.MachineModel))
                {
                    errors.Add($"第{rowNumber}行：机型不能为空");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.BomProcessorName))
                {
                    errors.Add($"第{rowNumber}行：BOM处理人不能为空");
                    continue;
                }

                // 如果BOM处理日期有值，但BOM处理人为空，给出警告（不阻断）
                if (row.BomProcessDate.HasValue && string.IsNullOrWhiteSpace(row.BomProcessorName))
                {
                    errors.Add($"第{rowNumber}行：BOM处理日期有值，但BOM处理人为空（已跳过此行）");
                    continue;
                }

                // 验证工时（如果填写了，必须是正数）
                if (row.WorkingHours.HasValue && row.WorkingHours.Value < 0)
                {
                    errors.Add($"第{rowNumber}行：工时必须>=0");
                    continue;
                }

                // 验证月份逻辑（如果填了，必须在1-12之间）
                if (row.Month.HasValue && (row.Month.Value < 1 || row.Month.Value > 12))
                {
                    errors.Add($"第{rowNumber}行：月份必须在1-12之间");
                    continue;
                }

                // 如果月份为空，根据接单日期自动计算
                if (!row.Month.HasValue && row.OrderDate != DateTime.MinValue)
                {
                    row.Month = row.OrderDate.Month;
                }

                validDtos.Add(row);
            }

            if (validDtos.Count == 0)
            {
                return BadRequest(new { success = false, message = "没有有效数据可导入" });
            }

            // 5. 保存到数据库
            var userInfos = await userInfoService.ListAsync(expression: src => src.RoleInfo!.Code == "GCJS");
            var entities = new List<ProcessingStandard>();
            foreach (var dto in validDtos)
            {
                int bomProcessorId = userInfos.FirstOrDefault(u => u.Name == dto.BomProcessorName)?.Id ?? 0;
                if (bomProcessorId == 0)
                {
                    errors.Add($"BOM处理人 '{dto.BomProcessorName}' 不存在，已跳过此行");
                    continue;
                }
                var entity = mapper.Map<ProcessingStandard>(dto);
                entity.BomProcessorId = bomProcessorId;
                entity.HandlerId = userInfos.FirstOrDefault(u => u.Name == dto.HandlerName)?.Id ?? null;
                entities.Add(entity);
            }

            //return await processingStandardService.AddAsync(entities).ContinueWith<IActionResult>(t => t.IsCompletedSuccessfully ? Ok(new
            //{
            //    success = true,
            //    message = $"成功导入{entities.Count}条记录",
            //    errors = errors.Count > 0 ? errors : null
            //}) : BadRequest(new
            //{
            //    success = false,
            //    message = "保存到数据库失败"
            //}));

            bool success = await processingStandardService.AddAsync(entities);
            return success ? Ok(new
            {
                success = true,
                message = $"成功导入{entities.Count}条记录",
                errors = errors.Count > 0 ? errors : null
            }) : BadRequest(new
            {
                success = false,
                message = "保存到数据库失败",
                errors = errors.Count > 0 ? errors : null
            });
        }

        /// <summary>
        /// Excel导出
        /// </summary>
        /// <returns></returns>
        [HttpPost("export")]
        [PermissionAsync("processing_standard_management", "management", "电脑")]
        public async Task<IActionResult> ExportToExcelAsync()
        {
            var entities = await processingStandardService.ListAsync(include: src => src.Include(src => src.BomProcessor!).Include(src => src.Handler!));

            if (entities is null || !entities.Any())
                return NotFound("没有数据可导出");

            var dtos = mapper.Map<List<ProcessingStandardDto>>(entities);
            var fileName = $"加工标准_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var memoryStream = new MemoryStream();
            await MiniExcel.SaveAsAsync(memoryStream, dtos, sheetName: "加工标准");
            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="dto">JSON</param>
        /// <returns></returns>
        [HttpPost("save")]
        [PermissionAsync("processing_standard_management", "management", "电脑")]
        public async Task<IActionResult> SaveAsync([FromBody] ProcessingStandardDto dto)
        {
            var entity = mapper.Map<ProcessingStandard>(dto);
            bool success = entity.Id > 0 ? await processingStandardService.UpdateAsync(entity) : await processingStandardService.AddAsync(entity);
            return success ? Ok(new { success, message = "保存成功" }) : BadRequest(new { success, message = "保存失败" });
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("remove/{id}")]
        [PermissionAsync("processing_standard_management", "management", "电脑")]
        public async Task<IActionResult> RemoveAsync([FromRoute] int id)
        {
            var entity = await processingStandardService.ModelAsync(expression: src => src.Id == id);

            if (entity is null)
                return NotFound("记录不存在或已删除");

            return await processingStandardService.RemoveAsync(entity).ContinueWith<IActionResult>(t => t.IsCompletedSuccessfully ? Ok(new
            {
                success = true,
                message = "删除成功"
            }) : BadRequest(new
            {
                success = false,
                message = "删除失败",
                error = t.Exception?.Message
            }));
        }

        /// <summary>
        /// 判断一行是否为空（所有字段都为空或默认值）
        /// </summary>
        private static bool IsRowEmpty(ProcessingStandardDto row)
        {
            return string.IsNullOrWhiteSpace(row.ToolName)
                && string.IsNullOrWhiteSpace(row.MaterialNumber)
                && string.IsNullOrWhiteSpace(row.ToolSpecification)
                && string.IsNullOrWhiteSpace(row.DrawingType)
                && string.IsNullOrWhiteSpace(row.ToolType)
                && string.IsNullOrWhiteSpace(row.MachineModel)
                && string.IsNullOrWhiteSpace(row.BomProcessorName)
                && string.IsNullOrWhiteSpace(row.HandlerName)
                && string.IsNullOrWhiteSpace(row.ProgramStatus)
                && string.IsNullOrWhiteSpace(row.Process)
                && string.IsNullOrWhiteSpace(row.StandardGrindingWheelSet)
                && string.IsNullOrWhiteSpace(row.CustomerDrawingNumber)
                && row.OrderDate == DateTime.MinValue
                && !row.BomProcessDate.HasValue
                && !row.CompletionDate.HasValue
                && !row.PlannedDemandTime.HasValue
                && !row.WorkingHours.HasValue
                && !row.Month.HasValue;
        }
    }
}
