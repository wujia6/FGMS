using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class EquipmentRepository : BaseRepository<Equipment>, IEquipmentRepository
    {
        public EquipmentRepository(IFgmsDbRepository<Equipment> repository) : base(repository)
        {
        }
    }
}
