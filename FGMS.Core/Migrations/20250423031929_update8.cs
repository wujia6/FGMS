using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "e7369dc1d82244b7",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "c49a8792cb454fdc");

            migrationBuilder.AddColumn<string>(
                name: "AgvStatus",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "execute");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgvStatus",
                table: "WorkOrders");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "c49a8792cb454fdc",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "e7369dc1d82244b7");
        }
    }
}
