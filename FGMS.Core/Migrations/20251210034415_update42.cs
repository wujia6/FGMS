using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update42 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MxCode",
                table: "MaterialIssueOrders",
                newName: "MxWareHouse");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "2c5e4ecaeca04164",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "7a72006aa0d54227");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "MaterialIssueOrders",
                type: "int",
                nullable: true,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "MxBarCode",
                table: "MaterialIssueOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MxCargoSpace",
                table: "MaterialIssueOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MxBarCode",
                table: "MaterialIssueOrders");

            migrationBuilder.DropColumn(
                name: "MxCargoSpace",
                table: "MaterialIssueOrders");

            migrationBuilder.RenameColumn(
                name: "MxWareHouse",
                table: "MaterialIssueOrders",
                newName: "MxCode");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "7a72006aa0d54227",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "2c5e4ecaeca04164");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "MaterialIssueOrders",
                type: "int",
                nullable: true,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 2);
        }
    }
}
