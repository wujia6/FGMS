using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update66 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DynamicBalance",
                table: "Components",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProcessingStandards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToolName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaterialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToolSpecification = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DrawingType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ToolType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MachineModel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BomProcessorId = table.Column<int>(type: "int", nullable: false),
                    BomProcessDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HandlerId = table.Column<int>(type: "int", nullable: true),
                    ProgramStatus = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Process = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlannedDemandTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkingHours = table.Column<double>(type: "float", nullable: true),
                    StandardGrindingWheelSet = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerDrawingNumber = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Month = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingStandards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessingStandards_UserInfos_BomProcessorId",
                        column: x => x.BomProcessorId,
                        principalTable: "UserInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessingStandards_UserInfos_HandlerId",
                        column: x => x.HandlerId,
                        principalTable: "UserInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingStandards_BomProcessorId",
                table: "ProcessingStandards",
                column: "BomProcessorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingStandards_HandlerId",
                table: "ProcessingStandards",
                column: "HandlerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessingStandards");

            migrationBuilder.DropColumn(
                name: "DynamicBalance",
                table: "Components");
        }
    }
}
