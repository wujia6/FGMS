using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update22 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "646d2ec8ef6b402a",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "1a6cd221309046f6");

            //migrationBuilder.AddColumn<string>(
            //    name: "Position",
            //    table: "ElementEntities",
            //    type: "nvarchar(10)",
            //    maxLength: 10,
            //    nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "Position",
            //    table: "ElementEntities");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "1a6cd221309046f6",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "646d2ec8ef6b402a");
        }
    }
}
