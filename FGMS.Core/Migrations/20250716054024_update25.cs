using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update25 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "1b0190feea91484e",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "1eac8b95bd2d4b28");

            migrationBuilder.AlterColumn<string>(
                name: "UpperJson",
                table: "ComponentLogs",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownJson",
                table: "ComponentLogs",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
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
                defaultValue: "1eac8b95bd2d4b28",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "1b0190feea91484e");

            migrationBuilder.AlterColumn<string>(
                name: "UpperJson",
                table: "ComponentLogs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownJson",
                table: "ComponentLogs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 5000,
                oldNullable: true);
        }
    }
}
