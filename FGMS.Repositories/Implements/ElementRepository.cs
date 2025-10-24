using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class ElementRepository : BaseRepository<Element>, IElementRepository
    {
        public ElementRepository(IFgmsDbRepository<Element> repository) : base(repository)
        {
        }
    }
}
