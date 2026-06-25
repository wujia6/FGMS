using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update63 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_ProductionOrders_ProductionOrderId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_ProductionOrderId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ProductionOrderId",
                table: "WorkOrders");

            migrationBuilder.AddColumn<int>(
                name: "WorkOrderId",
                table: "ProductionOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_WorkOrderId",
                table: "ProductionOrders",
                column: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrders_WorkOrders_WorkOrderId",
                table: "ProductionOrders",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrders_WorkOrders_WorkOrderId",
                table: "ProductionOrders");

            migrationBuilder.DropIndex(
                name: "IX_ProductionOrders_WorkOrderId",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                table: "ProductionOrders");

            migrationBuilder.AddColumn<int>(
                name: "ProductionOrderId",
                table: "WorkOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductionOrderId",
                table: "WorkOrders",
                column: "ProductionOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_ProductionOrders_ProductionOrderId",
                table: "WorkOrders",
                column: "ProductionOrderId",
                principalTable: "ProductionOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
