using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update19 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "696b461c637f4290",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "22abc09e3c1e4ef6");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "Equipments",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Mount",
                table: "Equipments",
                type: "bit",
                nullable: true,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mount",
                table: "Equipments");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "22abc09e3c1e4ef6",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "696b461c637f4290");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "Equipments",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);
        }
    }
}
