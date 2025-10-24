using FGMS.Models.Entities;

namespace FGMS.Services.Interfaces
{
    public interface IComponentService : IBaseService<Component>
    {
        public Task<bool> CombinedAsync(Component entity);

        public Task<dynamic> SplitAsync(int componentId);
    }
}
