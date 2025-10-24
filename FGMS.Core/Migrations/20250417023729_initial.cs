using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pid = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizes_Organizes_Pid",
                        column: x => x.Pid,
                        principalTable: "Organizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrackLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Elements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    MaterialNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModalNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    Spec = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Diameter = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WheelWidth = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RingWidth = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Thickness = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Angle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    InnerBoreDiameter = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Granularity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Binders = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Desc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elements_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CargoSpaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizeId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoSpaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargoSpaces_CargoSpaces_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CargoSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CargoSpaces_Organizes_OrganizeId",
                        column: x => x.OrganizeId,
                        principalTable: "Organizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipments_Organizes_OrganizeId",
                        column: x => x.OrganizeId,
                        principalTable: "Organizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleInfos_Organizes_OrganizeId",
                        column: x => x.OrganizeId,
                        principalTable: "Organizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Standards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MainElementId = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<int>(type: "int", nullable: false),
                    FirstRangle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstElementId = table.Column<int>(type: "int", nullable: false),
                    SecondRangle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SecondElementId = table.Column<int>(type: "int", nullable: false),
                    ThirdRangle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ThirdElementId = table.Column<int>(type: "int", nullable: false),
                    FourthRangle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FourthElementId = table.Column<int>(type: "int", nullable: false),
                    FifthRangle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FifthElementId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Standards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Standards_Elements_FifthElementId",
                        column: x => x.FifthElementId,
                        principalTable: "Elements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Standards_Elements_FirstElementId",
                        column: x => x.FirstElementId,
                        principalTable: "Elements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Standards_Elements_FourthElementId",
                        column: x => x.FourthElementId,
                        principalTable: "Elements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Standards_Elements_MainElementId",
                        column: x => x.MainElementId,
                        principalTable: "Elements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Standards_Elements_SecondElementId",
                        column: x => x.SecondElementId,
                        principalTable: "Elements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Standards_Elements_ThirdElementId",
                        column: x => x.ThirdElementId,
                        principalTable: "Elements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleInfoId = table.Column<int>(type: "int", nullable: false),
                    WorkNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInfos_RoleInfos_RoleInfoId",
                        column: x => x.RoleInfoId,
                        principalTable: "RoleInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pid = table.Column<int>(type: "int", nullable: true),
                    EquipmentId = table.Column<int>(type: "int", nullable: false),
                    UserInfoId = table.Column<int>(type: "int", nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    MaterialNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaterialSpec = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    Remark = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrders_UserInfos_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "UserInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_WorkOrders_Pid",
                        column: x => x.Pid,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Components",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StandardId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderId = table.Column<int>(type: "int", nullable: true),
                    CargoSpaceId = table.Column<int>(type: "int", nullable: true),
                    CargoSpaceHistory = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsStandard = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Components_CargoSpaces_CargoSpaceId",
                        column: x => x.CargoSpaceId,
                        principalTable: "CargoSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Components_Standards_StandardId",
                        column: x => x.StandardId,
                        principalTable: "Standards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Components_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ElementEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ElementId = table.Column<int>(type: "int", nullable: false),
                    ComponentId = table.Column<int>(type: "int", nullable: true),
                    CargoSpaceId = table.Column<int>(type: "int", nullable: true),
                    CargoSpaceHistory = table.Column<int>(type: "int", nullable: true),
                    MaterialNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BigDiameter = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SmallDiameter = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    InnerDiameter = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OuterDiameter = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AxialRunout = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RadialRunout = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Width = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SmallRangle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PlaneWidth = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BigRangle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    QrCodeImage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsGroup = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    BeginTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UseDuration = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElementEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElementEntities_CargoSpaces_CargoSpaceId",
                        column: x => x.CargoSpaceId,
                        principalTable: "CargoSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElementEntities_Components_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElementEntities_Elements_ElementId",
                        column: x => x.ElementId,
                        principalTable: "Elements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargoSpaces_OrganizeId",
                table: "CargoSpaces",
                column: "OrganizeId");

            migrationBuilder.CreateIndex(
                name: "IX_CargoSpaces_ParentId",
                table: "CargoSpaces",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_CargoSpaceId",
                table: "Components",
                column: "CargoSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_StandardId",
                table: "Components",
                column: "StandardId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_WorkOrderId",
                table: "Components",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ElementEntities_CargoSpaceId",
                table: "ElementEntities",
                column: "CargoSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ElementEntities_ComponentId",
                table: "ElementEntities",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ElementEntities_ElementId",
                table: "ElementEntities",
                column: "ElementId");

            migrationBuilder.CreateIndex(
                name: "IX_Elements_BrandId",
                table: "Elements",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_OrganizeId",
                table: "Equipments",
                column: "OrganizeId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizes_Pid",
                table: "Organizes",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "IX_RoleInfos_OrganizeId",
                table: "RoleInfos",
                column: "OrganizeId");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_FifthElementId",
                table: "Standards",
                column: "FifthElementId");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_FirstElementId",
                table: "Standards",
                column: "FirstElementId");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_FourthElementId",
                table: "Standards",
                column: "FourthElementId");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_MainElementId",
                table: "Standards",
                column: "MainElementId");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_SecondElementId",
                table: "Standards",
                column: "SecondElementId");

            migrationBuilder.CreateIndex(
                name: "IX_Standards_ThirdElementId",
                table: "Standards",
                column: "ThirdElementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfos_RoleInfoId",
                table: "UserInfos",
                column: "RoleInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_EquipmentId",
                table: "WorkOrders",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_Pid",
                table: "WorkOrders",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_UserInfoId",
                table: "WorkOrders",
                column: "UserInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElementEntities");

            migrationBuilder.DropTable(
                name: "TrackLogs");

            migrationBuilder.DropTable(
                name: "Components");

            migrationBuilder.DropTable(
                name: "CargoSpaces");

            migrationBuilder.DropTable(
                name: "Standards");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "Elements");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "UserInfos");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "RoleInfos");

            migrationBuilder.DropTable(
                name: "Organizes");
        }
    }
}
