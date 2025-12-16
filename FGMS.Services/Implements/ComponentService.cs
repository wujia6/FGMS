using System.Linq.Expressions;
using FGMS.Core.EfCore.Implements;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Models;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FGMS.Services.Implements
{
    internal class ComponentService : BaseService<Component>, IComponentService
    {
        private readonly IComponentRepository? componentRepository;
        private readonly IElementEntityRepository? elementEntityRepository;
        private readonly ITrackLogRepository? trackLogRepository;
        private readonly IFgmsDbContext? context;

        public ComponentService(IBaseRepository<Component> repo,IFgmsDbContext context) : base(repo, context)
        {
        }

        public ComponentService(IBaseRepository<Component> repo,IFgmsDbContext context,IElementEntityRepository elementEntityRepository,ITrackLogRepository trackLogRepository) : base(repo, context)
        {
            this.componentRepository = repo as IComponentRepository;
            this.elementEntityRepository = elementEntityRepository;
            this.trackLogRepository = trackLogRepository;
            this.context = context;
        }

        public async Task<bool> CombinedAsync(Component entity)
        {
            if (entity.ElementEntities is null)
                return false;

            await context!.BeginTrans();
            try
            {
                componentRepository!.AddEntity(entity);
                entity.ElementEntities.ToList().ForEach(x => x.ComponentId = entity.Id);
                elementEntityRepository!.UpdateEntity(entity.ElementEntities!, new Expression<Func<ElementEntity, object>>[] { src => src.ComponentId!, src => src.IsGroup });
                await context.SaveChangesAsync();
                await context.CommitTrans();
                return true;
            }
            catch (Exception ex)
            {
                await context.RollBackTrans();
                throw new Exception("Combine Error " + ex.Message);
            }
        }

        public async Task<dynamic> SplitAsync(int componentId)
        {
            await context!.BeginTrans();
            try
            {
                var cmpEntity = await componentRepository!.GetEntityAsync(expression: src => src.Id == componentId, include: src => src.Include(src => src.Standard!).Include(src => src.ElementEntities!));
                var ees = cmpEntity.ElementEntities!.ToList();

                if (ees.Any(src => src.Status != ElementEntityStatus.在库))
                    return new { success = false, message = "砂轮组元件状态是“在库”才能进行拆分!" };

                foreach (var ee in ees)
                {
                    //获取散件货位信息
                    if(!ee.CargoSpaceHistory.HasValue)
                    {
                        return new { success = false, message = $"{ee.MaterialNo}缺少散件货位信息" };
                    }
                    //更新工件componentId字段
                    ee.Component = null;
                    ee.ComponentId = new int?();
                    ee.CargoSpaceId = ee.CargoSpaceHistory.Value;
                    ee.Status = Models.ElementEntityStatus.在库;
                    ee.IsGroup = false;
                    string msg = cmpEntity.IsStandard ? $"标组：{cmpEntity.Standard!.Code}拆分，{ee.MaterialNo}入库散件货位" : $"非标组({cmpEntity.Id})拆分，{ee.MaterialNo}入库散件货位";
                    trackLogRepository!.AddEntity(new TrackLog { Content = msg });
                }
                elementEntityRepository!.UpdateEntity(ees, new Expression<Func<ElementEntity, object>>[] { src => src.ComponentId!, src => src.CargoSpaceId!, src => src.Status, src => src.IsGroup });
                //删除组件
                componentRepository.DeleteEntity(cmpEntity);
                bool success = await context.SaveChangesAsync() > 0;
                if (success)
                    await context.CommitTrans();
                else
                    await context.RollBackTrans();
                return new { success, message = success ? "拆分入库成功" : "拆分入库失败" };
            }
            catch (Exception ex)
            {
                await context.RollBackTrans();
                throw new Exception("Updating Error " + ex.Message);
            }
        }
    }
}
