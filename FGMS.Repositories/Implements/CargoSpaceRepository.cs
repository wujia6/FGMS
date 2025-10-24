using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class CargoSpaceRepository : BaseRepository<CargoSpace>, ICargoSpaceRepository
    {
        public CargoSpaceRepository(IFgmsDbRepository<CargoSpace> repository) : base(repository)
        {
        }
    }
}
