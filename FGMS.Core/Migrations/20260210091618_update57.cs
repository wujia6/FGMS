using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update57 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedTime",
                table: "ProductionOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedEndTime",
                table: "ProductionOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "FyStandardMinutes",
                table: "MaterialDiameters",
                type: "float(6)",
                precision: 6,
                scale: 2,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedTime",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "PlannedEndTime",
                table: "ProductionOrders");

            migrationBuilder.AlterColumn<double>(
                name: "FyStandardMinutes",
                table: "MaterialDiameters",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float(6)",
                oldPrecision: 6,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
