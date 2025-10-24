using System.Linq.Expressions;
using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class StandardService : BaseService<Standard>, IStandardService
    {
        //private readonly IBaseRepository<Standard>? standardRepository;
        //private readonly IBaseRepository<ElementEntity>? elementEntityRepository;
        //private readonly IFgmsDbContext? fgmsDbContext;

        public StandardService(IBaseRepository<Standard> repo, IFgmsDbContext context) : base(repo, context)
        {
        }

        //public StandardService(IBaseRepository<Standard>? standardRepository, IBaseRepository<ElementEntity>? elementEntityRepository, IFgmsDbContext? fgmsDbContext) : base(standardRepository!, fgmsDbContext!)
        //{
        //    this.standardRepository = standardRepository;
        //    this.elementEntityRepository = elementEntityRepository;
        //    this.fgmsDbContext = fgmsDbContext;
        //}

        //public async Task<dynamic> SaveAsync(Standard entity)
        //{
        //    await fgmsDbContext!.BeginTrans();
        //    try
        //    {
        //        if (entity.Id > 0)
        //        {

        //        }
        //        else
        //        {
        //            standardRepository!.AddEntity(entity);
        //            var ids = Array.Empty<int>();
        //            ids[0] = entity.MainElementEntityId;
        //            ids[1] = entity.FirstElementEntityId!.Value;
        //            ids[2] = entity.SecondElementEntityId!.Value;
        //            ids[3] = entity.ThirdElementEntityId!.Value;
        //            ids[4] = entity.FourthElementEntityId!.Value;
        //            ids[5] = entity.FifthElementEntityId!.Value;
        //            if (ids.Length > 0)
        //            {
        //                var ees = await elementEntityRepository!.GetListAsync(expression: src => ids.Contains(src.Id));
        //                ees.ToList().ForEach(x => x.IsGroup = true);
        //                elementEntityRepository.UpdateEntity(ees, new Expression<Func<ElementEntity, object>>[] { src => src.IsGroup });
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
            
        //}
    }
}
