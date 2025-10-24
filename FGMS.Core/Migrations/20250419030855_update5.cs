using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "c08e0e6015c24f96",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "fe93ffed979d40e2");

            migrationBuilder.AddColumn<string>(
                name: "CurrentAngle",
                table: "ElementEntities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentAngle",
                table: "ElementEntities");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "fe93ffed979d40e2",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "c08e0e6015c24f96");
        }
    }
}
