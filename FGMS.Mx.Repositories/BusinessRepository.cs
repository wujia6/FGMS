using FGMS.Mx.Core;
using FGMS.Mx.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace FGMS.Mx.Repositories
{
    internal class BusinessRepository : IBusinessRepository
    {
        private readonly IMxDbContext mxDbContext;

        public BusinessRepository(IMxDbContext mxDbContext)
        {
            this.mxDbContext = mxDbContext;
        }

        public async Task<List<OutboundMaterial>> GetBarcodesAsync(string codes)
        {
            //var codeList = codes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            var sql = $@"
                SELECT c.`code` as ProductionOrderCode, a.label_code as BarCode
                FROM cr_out_store_application_many a
                LEFT JOIN cr_out_store_application b ON a.oneId = b.id
                LEFT JOIN sc_production_order c ON b.productionOrderId = c.id
                WHERE c.`code` IN ({codes})";

            return await mxDbContext.OutboundMaterials.FromSqlRaw(sql).ToListAsync();
        }

        public async Task<StoragePosition> GetStoragePositionsAsync(string code)
        {
            var sql = @"
                select
	                c.`code` as ProductionOrderCode,
	                a.`code` as OutStoreOrderCode,
	                d.`name` as Warehouse,
	                group_concat(distinct f.`name` separator ', ') as CargoSpace
                from cr_out_store_application a
                left join cr_out_store_application_many b on b.oneId=a.id
                left join sc_production_order c on a.productionOrderId=c.id
                left join cr_warehouse_management d on a.warehouseId=d.id
                left join cp_material_management e on b.materialId=e.id
                left join (
	                select distinct a.materialId, c.`name`
	                from cr_inventory_detail a
	                left join cp_material_management b ON a.materialId = b.id
	                left join cr_goods_allocation_management c ON a.allocationId = c.id
                ) f on e.id=f.materialId
                where a.productionOrderId is not null and a.delete_time is null and c.`code`={0}
                group by c.`code`,d.`name`";
            return await mxDbContext.StoragePositions.FromSqlRaw(sql, code).FirstOrDefaultAsync();
        }

        public async Task UpdateProductionOrderStatus(string poNo, string status)
        {
            string sql = "update sc_production_order set state=@state where code=@code";
            var sqlParams = new List<MySqlParameter>
            {
                new("@state", status),
                new("@code", poNo)
            };
            await mxDbContext.DataBase.ExecuteSqlRawAsync(sql, sqlParams);
        }

        public async Task<WorkReport> ReportSummaryAsync(string strWhere)
        {
            string sql = $@"select sum(good_products) goodNum, sum(total_defective_products) failNum, round(sum(total_defective_products)/sum(total_complete_quantity)*100,2) failRatio 
                from sc_report_work 
                where processId=1 and delete_time is null {strWhere}";

            return await mxDbContext.WorkReports.FromSqlRaw(sql).FirstOrDefaultAsync();
        }
    }
}
