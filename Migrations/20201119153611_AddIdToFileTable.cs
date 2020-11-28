using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Private_Note.Migrations
{
    public partial class AddIdToFileTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Files",
                table: "Files");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Files",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<byte[]>(
                name: "File",
                table: "Files",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(900)");

            migrationBuilder.AlterColumn<string>(
                name: "FileType",
                table: "Files",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Files",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Files",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Files",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Files",
                table: "Files",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Files",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Files");

            migrationBuilder.AlterColumn<string>(
                name: "FileType",
                table: "Files",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Files",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "File",
                table: "Files",
                type: "varbinary(900)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Files",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Files",
                table: "Files",
                columns: new[] { "FileName", "FileType", "File", "CreatedDate" });
        }
    }
}
