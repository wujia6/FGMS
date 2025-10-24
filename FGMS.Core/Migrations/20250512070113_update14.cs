using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update14 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                oldDefaultValue: "be7e4ceaac3342b0");

            migrationBuilder.CreateTable(
                name: "AgvTaskSyncs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgvCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TaskCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    WorkOrderNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Start = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    End = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PositionX = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PositionY = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgvTaskSyncs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgvTaskSyncs");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "be7e4ceaac3342b0",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "2dda1e604fcb4014");
        }
    }
}
