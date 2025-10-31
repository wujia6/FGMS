﻿using FGMS.Models.Entities;

namespace FGMS.Services.Interfaces
{
    public interface IWorkOrderService : IBaseService<WorkOrder>
    {
        public Task<dynamic> ReceiveAsync(dynamic paramJson);

        public Task<dynamic> CancelAsync(int orderId);

        public Task<dynamic> ReadyAsync(dynamic paramJson);

        public Task<dynamic> RenovatedAsync(ElementEntity entity, string workOrderNo, int renovateorId);

        public Task<dynamic> ReadyActionAsync(dynamic paramJson);

        public Task<dynamic> AuditAsync(dynamic paramJson);

        public Task<dynamic> AuditPmcAsync(dynamic paramJson);

        public Task<dynamic> EquipmentChangeAsync(dynamic paramJson);
    }
}
