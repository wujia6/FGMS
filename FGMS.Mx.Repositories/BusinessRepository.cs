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
                SELECT 
                    b.code AS ProductionOrderCode,
                    c.`name` AS Warehouse,
                    GROUP_CONCAT(DISTINCT goods_allocation.`name` SEPARATOR ', ') AS CargoSpace
                FROM cr_out_store_application a 
                LEFT JOIN sc_production_order b ON a.productionOrderId = b.id
                LEFT JOIN cr_warehouse_management c ON a.warehouseId = c.id
                LEFT JOIN cp_material_management d ON b.materialId = d.id
                LEFT JOIN (
                    SELECT DISTINCT a.materialId, c.`name`
                    FROM cr_inventory_detail a
                    LEFT JOIN cp_material_management b ON a.materialId = b.id
                    LEFT JOIN cr_goods_allocation_management c ON a.allocationId = c.id
                ) goods_allocation ON d.id = goods_allocation.materialId
                WHERE a.productionOrderId IS NOT NULL 
                    AND a.delete_time IS NULL 
                    AND b.code = {0}
                GROUP BY b.code, c.`name`";

            return await mxDbContext.StoragePositions.FromSqlRaw(sql, code).FirstOrDefaultAsync();
        }

        public async Task UpdateProductionOrderStatus(string poNo, string status)
        {
            string sql = "update sc_production_order set status=@status where code=@code";
            var sqlParams = new List<MySqlParameter>
            {
                new("@status", status),
                new("@code", poNo)
            };
            await mxDbContext.DataBase.ExecuteSqlRawAsync(sql, sqlParams);
        }
    }
}
