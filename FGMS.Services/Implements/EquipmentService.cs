using FGMS.Core.EfCore.Interfaces;
using FGMS.Models.Entities;
using FGMS.Repositories.Interfaces;
using FGMS.Services.Interfaces;

namespace FGMS.Services.Implements
{
    internal class EquipmentService : BaseService<Equipment>, IEquipmentService
    {
        public EquipmentService(IBaseRepository<Equipment> repo, IFgmsDbContext context) : base(repo, context)
        {
        }
    }
}
