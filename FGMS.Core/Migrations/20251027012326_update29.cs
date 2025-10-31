using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update29 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "d797ff58a6dd472d",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "a6922fa763154fbe");

            migrationBuilder.AddColumn<int>(
                name: "DiscardBy",
                table: "ElementEntities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscardTime",
                table: "ElementEntities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkOrderEquipmentChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    OldEquipment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NewEquipmentId = table.Column<int>(type: "int", nullable: false),
                    RequestorId = table.Column<int>(type: "int", nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChangeTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderEquipmentChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderEquipmentChanges_Equipments_NewEquipmentId",
                        column: x => x.NewEquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderEquipmentChanges_UserInfos_RequestorId",
                        column: x => x.RequestorId,
                        principalTable: "UserInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderEquipmentChanges_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderEquipmentChanges_NewEquipmentId",
                table: "WorkOrderEquipmentChanges",
                column: "NewEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderEquipmentChanges_RequestorId",
                table: "WorkOrderEquipmentChanges",
                column: "RequestorId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderEquipmentChanges_WorkOrderId",
                table: "WorkOrderEquipmentChanges",
                column: "WorkOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderEquipmentChanges");

            migrationBuilder.DropColumn(
                name: "DiscardBy",
                table: "ElementEntities");

            migrationBuilder.DropColumn(
                name: "DiscardTime",
                table: "ElementEntities");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "a6922fa763154fbe",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "d797ff58a6dd472d");
        }
    }
}
