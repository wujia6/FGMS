using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update23 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "32a2f9ca52134261",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "646d2ec8ef6b402a");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "TrackLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "ComponentLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaterialNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaterialSpec = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EquipmentCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RequiredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpperJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DownJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentLogs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComponentLogs");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "646d2ec8ef6b402a",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "32a2f9ca52134261");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "TrackLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
