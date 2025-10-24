using FGMS.Models.Entities;

namespace FGMS.Services.Interfaces
{
    public interface IWorkOrderService : IBaseService<WorkOrder>
    {
        public Task<dynamic> ReceiveAsync(dynamic paramJson);

        public Task<dynamic> CancelAsync(int orderId);

        public Task<dynamic> ReadyAsync(dynamic paramJson);

        public Task<dynamic> ReadyActionAsync(dynamic paramJson);

        public Task<dynamic> AuditAsync(dynamic paramJson);
    }
}
