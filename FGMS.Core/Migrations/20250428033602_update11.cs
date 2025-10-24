using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "8955fa03490b421a",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "45b6213b05b04234");

            migrationBuilder.AlterColumn<float>(
                name: "UseDuration",
                table: "ElementEntities",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
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
                defaultValue: "45b6213b05b04234",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "8955fa03490b421a");

            migrationBuilder.AlterColumn<float>(
                name: "UseDuration",
                table: "ElementEntities",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldDefaultValue: 0f);
        }
    }
}
