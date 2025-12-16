using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update37 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Diameter",
                table: "ProductionOrders");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "5155bd901a3c4ad6",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "2cb16fb28f9d4f27");

            migrationBuilder.AddColumn<double>(
                name: "WorkHours",
                table: "ProductionOrders",
                type: "float(6)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MaterialDiameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Diameter = table.Column<double>(type: "float(6)", precision: 6, scale: 3, nullable: false),
                    StandardMinutes = table.Column<double>(type: "float(6)", precision: 6, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialDiameters", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialDiameters");

            migrationBuilder.DropColumn(
                name: "WorkHours",
                table: "ProductionOrders");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "2cb16fb28f9d4f27",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "5155bd901a3c4ad6");

            migrationBuilder.AddColumn<decimal>(
                name: "Diameter",
                table: "ProductionOrders",
                type: "decimal(6,3)",
                precision: 6,
                scale: 3,
                nullable: true);
        }
    }
}
