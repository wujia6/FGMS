using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class ElementEntityRepository : BaseRepository<ElementEntity>, IElementEntityRepository
    {
        public ElementEntityRepository(IFgmsDbRepository<ElementEntity> repository) : base(repository)
        {
        }
    }
}
