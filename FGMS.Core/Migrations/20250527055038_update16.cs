using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update16 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "b462a3d36cce4526",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "041db91034bd4f3d");

            migrationBuilder.CreateTable(
                name: "WorkOrderStandard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    StandardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderStandard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderStandard_Standards_StandardId",
                        column: x => x.StandardId,
                        principalTable: "Standards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderStandard_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderStandard_StandardId",
                table: "WorkOrderStandard",
                column: "StandardId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderStandard_WorkOrderId",
                table: "WorkOrderStandard",
                column: "WorkOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderStandard");

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
                oldDefaultValue: "b462a3d36cce4526");
        }
    }
}
