using Microsoft.EntityFrameworkCore.Migrations;

namespace Private_Note.Migrations
{
    public partial class updatesizefoSecretPasswordto200 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SecretPassword",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 16,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SecretPassword",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 16,
                oldNullable: true);
        }
    }
}
