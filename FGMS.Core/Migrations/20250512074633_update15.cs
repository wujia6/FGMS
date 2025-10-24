using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update15 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionX",
                table: "AgvTaskSyncs");

            migrationBuilder.DropColumn(
                name: "PositionY",
                table: "AgvTaskSyncs");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "041db91034bd4f3d",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "2dda1e604fcb4014");

            migrationBuilder.AlterColumn<string>(
                name: "WorkOrderNo",
                table: "AgvTaskSyncs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TaskCode",
                table: "AgvTaskSyncs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Start",
                table: "AgvTaskSyncs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "End",
                table: "AgvTaskSyncs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "2dda1e604fcb4014",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "041db91034bd4f3d");

            migrationBuilder.AlterColumn<string>(
                name: "WorkOrderNo",
                table: "AgvTaskSyncs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "TaskCode",
                table: "AgvTaskSyncs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Start",
                table: "AgvTaskSyncs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "End",
                table: "AgvTaskSyncs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<string>(
                name: "PositionX",
                table: "AgvTaskSyncs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionY",
                table: "AgvTaskSyncs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }
    }
}
