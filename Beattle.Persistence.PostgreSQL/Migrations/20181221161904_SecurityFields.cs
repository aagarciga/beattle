using Microsoft.EntityFrameworkCore.Migrations;

namespace Beattle.Persistence.PostgreSQL.Migrations
{
    public partial class SecurityFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "User",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Settings",
                table: "User",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Role",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Settings",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Role");
        }
    }
}
