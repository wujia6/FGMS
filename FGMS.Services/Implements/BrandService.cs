using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class BrandService : BaseService<Brand>, IBrandService
    {
        public BrandService(IBaseRepository<Brand> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
