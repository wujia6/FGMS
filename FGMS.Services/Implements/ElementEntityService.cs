using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class ElementEntityService : BaseService<ElementEntity>, IElementEntityService
    {
        public ElementEntityService(IBaseRepository<ElementEntity> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
