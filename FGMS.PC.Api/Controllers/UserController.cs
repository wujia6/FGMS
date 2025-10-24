﻿using FGMS.Models.Dtos;
using FGMS.Models.Entities;
using FGMS.Services.Interfaces;
using FGMS.Utils;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FGMS.PC.Api.Controllers
{
    /// <summary>
    /// 用户接口
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("fgms/pc/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserInfoService userInfoService;
        private readonly IMapper mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userInfoService"></param>
        /// <param name="mapper"></param>
        public UserController(IUserInfoService userInfoService, IMapper mapper)
        {
            this.userInfoService = userInfoService;
            this.mapper = mapper;
        }

        /// <summary>
        /// 集合
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">记录数</param>
        /// <param name="name">姓名</param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<dynamic> ListAsync(int? pageIndex, int? pageSize, string? name)
        {
            var expression = ExpressionBuilder.GetTrue<UserInfo>();
            if (!string.IsNullOrEmpty(name))
                expression = expression.And(src => src.Name.Contains(name));
            var entities = await userInfoService.ListAsync(expression, include: src => src.Include(src => src.RoleInfo!).ThenInclude(src => src.Organize!));
            int total = entities.Count;
            if (pageIndex.HasValue && pageSize.HasValue)
                entities = entities.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            return new { total, rows = mapper.Map<List<UserInfoDto>>(entities) };
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="model">JSON</param>
        /// <returns></returns>
        [HttpPost("save")]
        public async Task<dynamic> SaveAsync([FromBody] UserInfoDto model)
        {
            var entity = mapper.Map<UserInfo>(model);
            bool success = entity.Id > 0 ? await userInfoService.UpdateAsync(entity) : await userInfoService.AddAsync(entity);
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
            int userId = param!.id;
            var entity = await userInfoService.ModelAsync(expression: src => src.Id == userId);
            if (entity == null) return new { success = false, message = "记录不存在或已删除" };
            bool success = await userInfoService.RemoveAsync(entity);
            return new { success, message = success ? "删除成功" : "删除失败" };
        }
    }
}
