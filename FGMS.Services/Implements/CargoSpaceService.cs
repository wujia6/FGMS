using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class CargoSpaceService : BaseService<CargoSpace>, ICargoSpaceService
    {
        public CargoSpaceService(IBaseRepository<CargoSpace> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
