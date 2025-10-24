using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update20 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                defaultValue: "7fa72b6252944a6d",
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
                oldDefaultValue: "7fa72b6252944a6d");

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
    }
}
