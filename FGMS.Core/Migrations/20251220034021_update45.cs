using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update45 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "5de689139ae445d8",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "d94da24810e14f04");

            migrationBuilder.CreateTable(
                name: "MenuInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Client = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuInfos_MenuInfos_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MenuInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PermissionInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleInfoId = table.Column<int>(type: "int", nullable: false),
                    MenuInfoId = table.Column<int>(type: "int", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CanUpload = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CanDownload = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionInfos_MenuInfos_MenuInfoId",
                        column: x => x.MenuInfoId,
                        principalTable: "MenuInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionInfos_RoleInfos_RoleInfoId",
                        column: x => x.RoleInfoId,
                        principalTable: "RoleInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuInfos_ParentId",
                table: "MenuInfos",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionInfos_MenuInfoId",
                table: "PermissionInfos",
                column: "MenuInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionInfos_RoleInfoId",
                table: "PermissionInfos",
                column: "RoleInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionInfos");

            migrationBuilder.DropTable(
                name: "MenuInfos");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "d94da24810e14f04",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "5de689139ae445d8");
        }
    }
}
