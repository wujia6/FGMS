using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "45b6213b05b04234",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "7145f7e8b33f4381");

            migrationBuilder.AlterColumn<float>(
                name: "UseDuration",
                table: "ElementEntities",
                type: "real",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
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
                defaultValue: "7145f7e8b33f4381",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "45b6213b05b04234");

            migrationBuilder.AlterColumn<decimal>(
                name: "UseDuration",
                table: "ElementEntities",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);
        }
    }
}
