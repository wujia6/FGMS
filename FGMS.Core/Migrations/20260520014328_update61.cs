using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update61 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderLogs_UserInfos_OperatorId",
                table: "ProductionOrderLogs");

            migrationBuilder.DropIndex(
                name: "IX_ProductionOrderLogs_OperatorId",
                table: "ProductionOrderLogs");

            migrationBuilder.DropColumn(
                name: "OperatorId",
                table: "ProductionOrderLogs");

            migrationBuilder.AddColumn<string>(
                name: "Operator",
                table: "ProductionOrderLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Operator",
                table: "ProductionOrderLogs");

            migrationBuilder.AddColumn<int>(
                name: "OperatorId",
                table: "ProductionOrderLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderLogs_OperatorId",
                table: "ProductionOrderLogs",
                column: "OperatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderLogs_UserInfos_OperatorId",
                table: "ProductionOrderLogs",
                column: "OperatorId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
