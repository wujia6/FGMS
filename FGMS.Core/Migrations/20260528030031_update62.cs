using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update62 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_ProductionOrderId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                table: "ProductionOrders");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductionOrderId",
                table: "WorkOrders",
                column: "ProductionOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_ProductionOrderId",
                table: "WorkOrders");

            migrationBuilder.AddColumn<int>(
                name: "WorkOrderId",
                table: "ProductionOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductionOrderId",
                table: "WorkOrders",
                column: "ProductionOrderId",
                unique: true,
                filter: "[ProductionOrderId] IS NOT NULL");
        }
    }
}
