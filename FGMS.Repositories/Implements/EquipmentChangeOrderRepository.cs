using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;

namespace FGMS.Repositories.Implements
{
    internal class EquipmentChangeOrderRepository : BaseRepository<EquipmentChangeOrder>, IEquipmentChangeOrderRepository
    {
        public EquipmentChangeOrderRepository(IFgmsDbRepository<EquipmentChangeOrder> repository) : base(repository)
        {
        }
    }
}
