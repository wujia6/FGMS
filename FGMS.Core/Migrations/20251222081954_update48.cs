using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update48 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanDelete",
                table: "PermissionInfos");

            migrationBuilder.DropColumn(
                name: "CanDownload",
                table: "PermissionInfos");

            migrationBuilder.DropColumn(
                name: "CanEdit",
                table: "PermissionInfos");

            migrationBuilder.RenameColumn(
                name: "CanUpload",
                table: "PermissionInfos",
                newName: "CanManagement");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "c15898f0519d4c6b",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "9bc77b3701034945");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CanManagement",
                table: "PermissionInfos",
                newName: "CanUpload");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "9bc77b3701034945",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "c15898f0519d4c6b");

            migrationBuilder.AddColumn<bool>(
                name: "CanDelete",
                table: "PermissionInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDownload",
                table: "PermissionInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanEdit",
                table: "PermissionInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
