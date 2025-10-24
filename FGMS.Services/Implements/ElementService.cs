using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class ElementService : BaseService<Element>, IElementService
    {
        public ElementService(IBaseRepository<Element> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
