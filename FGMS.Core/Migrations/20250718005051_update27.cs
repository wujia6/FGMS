using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGMS.Core.Migrations
{
    public partial class update27 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "bb841a627e8046f9",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "0748391334084757");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Content",
            //    table: "TrackLogs",
            //    type: "nvarchar(200)",
            //    maxLength: 200,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(1000)",
            //    oldMaxLength: 1000);

            //migrationBuilder.AddColumn<string>(
            //    name: "JsonContent",
            //    table: "TrackLogs",
            //    type: "nvarchar(1000)",
            //    maxLength: 1000,
            //    nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "JsonContent",
            //    table: "TrackLogs");

            migrationBuilder.AlterColumn<string>(
                name: "AgvTaskCode",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "0748391334084757",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "bb841a627e8046f9");

            //migrationBuilder.AlterColumn<string>(
            //    name: "Content",
            //    table: "TrackLogs",
            //    type: "nvarchar(1000)",
            //    maxLength: 1000,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(200)",
            //    oldMaxLength: 200);
        }
    }
}
