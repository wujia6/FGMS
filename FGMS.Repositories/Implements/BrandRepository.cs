using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class BrandRepository : BaseRepository<Brand>, IBrandRepository
    {
        public BrandRepository(IFgmsDbRepository<Brand> repository) : base(repository)
        {
        }
    }
}
