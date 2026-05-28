using FGMS.Models.Entities;

namespace FGMS.Services.Interfaces
{
    public interface IWorkOrderService : IBaseService<WorkOrder>
    {
        Task<dynamic> ReceiveAsync(dynamic paramJson);

        Task<dynamic> CancelAsync(int orderId);

        Task<dynamic> ReadyAsync(dynamic paramJson);

        Task<dynamic> RenovatedAsync(ElementEntity entity, string workOrderNo, int renovateorId);

        Task<dynamic> ReadyActionAsync(dynamic paramJson);

        Task<dynamic> AuditAsync(dynamic paramJson);

        Task<dynamic> MachineUpperAsync(int orderId);

        Task<dynamic> MachineDownAsync(List<Component> components);

        Task<dynamic> WheelBackStockAsync(int woId);

        Task<dynamic> UnbindProductionAsync(string orderNo);
    }
}
