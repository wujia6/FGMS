using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update43 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialIssueOrders_UserInfos_UserInfoId",
                table: "MaterialIssueOrders");

            migrationBuilder.RenameColumn(
                name: "UserInfoId",
                table: "MaterialIssueOrders",
                newName: "SendorId");

            migrationBuilder.RenameIndex(
                name: "IX_MaterialIssueOrders_UserInfoId",
                table: "MaterialIssueOrders",
                newName: "IX_MaterialIssueOrders_SendorId");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "c7053710218e476c",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "2c5e4ecaeca04164");

            migrationBuilder.AddColumn<int>(
                name: "CreateorId",
                table: "MaterialIssueOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssueOrders_CreateorId",
                table: "MaterialIssueOrders",
                column: "CreateorId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialIssueOrders_UserInfos_CreateorId",
                table: "MaterialIssueOrders",
                column: "CreateorId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialIssueOrders_UserInfos_SendorId",
                table: "MaterialIssueOrders",
                column: "SendorId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialIssueOrders_UserInfos_CreateorId",
                table: "MaterialIssueOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_MaterialIssueOrders_UserInfos_SendorId",
                table: "MaterialIssueOrders");

            migrationBuilder.DropIndex(
                name: "IX_MaterialIssueOrders_CreateorId",
                table: "MaterialIssueOrders");

            migrationBuilder.DropColumn(
                name: "CreateorId",
                table: "MaterialIssueOrders");

            migrationBuilder.RenameColumn(
                name: "SendorId",
                table: "MaterialIssueOrders",
                newName: "UserInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_MaterialIssueOrders_SendorId",
                table: "MaterialIssueOrders",
                newName: "IX_MaterialIssueOrders_UserInfoId");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "2c5e4ecaeca04164",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "c7053710218e476c");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialIssueOrders_UserInfos_UserInfoId",
                table: "MaterialIssueOrders",
                column: "UserInfoId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
