using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update34 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderStandard_Standards_StandardId",
                table: "WorkOrderStandard");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderStandard_WorkOrders_WorkOrderId",
                table: "WorkOrderStandard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkOrderStandard",
                table: "WorkOrderStandard");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "WorkOrders");

            migrationBuilder.RenameTable(
                name: "WorkOrderStandard",
                newName: "WorkOrderStandards");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderStandard_WorkOrderId",
                table: "WorkOrderStandards",
                newName: "IX_WorkOrderStandards_WorkOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderStandard_StandardId",
                table: "WorkOrderStandards",
                newName: "IX_WorkOrderStandards_StandardId");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "d31decc6fd1a4c15",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "9eee4d85783a4b72");

            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "ProductionOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkOrderStandards",
                table: "WorkOrderStandards",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EquipmentChangeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserInfoId = table.Column<int>(type: "int", nullable: false),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false),
                    EquipmentId = table.Column<int>(type: "int", nullable: false),
                    OldEquipmentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentChangeOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentChangeOrders_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentChangeOrders_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentChangeOrders_UserInfos_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "UserInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentChangeOrders_EquipmentId",
                table: "EquipmentChangeOrders",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentChangeOrders_ProductionOrderId",
                table: "EquipmentChangeOrders",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentChangeOrders_UserInfoId",
                table: "EquipmentChangeOrders",
                column: "UserInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderStandards_Standards_StandardId",
                table: "WorkOrderStandards",
                column: "StandardId",
                principalTable: "Standards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderStandards_WorkOrders_WorkOrderId",
                table: "WorkOrderStandards",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderStandards_Standards_StandardId",
                table: "WorkOrderStandards");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderStandards_WorkOrders_WorkOrderId",
                table: "WorkOrderStandards");

            migrationBuilder.DropTable(
                name: "EquipmentChangeOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkOrderStandards",
                table: "WorkOrderStandards");

            migrationBuilder.RenameTable(
                name: "WorkOrderStandards",
                newName: "WorkOrderStandard");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderStandards_WorkOrderId",
                table: "WorkOrderStandard",
                newName: "IX_WorkOrderStandard_WorkOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderStandards_StandardId",
                table: "WorkOrderStandard",
                newName: "IX_WorkOrderStandard_StandardId");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "9eee4d85783a4b72",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "d31decc6fd1a4c15");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "WorkOrders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "ProductionOrders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkOrderStandard",
                table: "WorkOrderStandard",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderStandard_Standards_StandardId",
                table: "WorkOrderStandard",
                column: "StandardId",
                principalTable: "Standards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderStandard_WorkOrders_WorkOrderId",
                table: "WorkOrderStandard",
                column: "WorkOrderId",
                principalTable: "WorkOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
