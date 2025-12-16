using FGMS.Models.Entities;

namespace FGMS.Services.Interfaces
{
    public interface IAgvTaskSyncService : IBaseService<AgvTaskSync>
    {
        public Task<dynamic> ExecuteAgvTaskAsync(string taskType, string taskUrl, string taskCode, string? start = null, string? end = null);

        public Task CallbackAsync(string taskCode, string robotCode, string method);
    }
}
