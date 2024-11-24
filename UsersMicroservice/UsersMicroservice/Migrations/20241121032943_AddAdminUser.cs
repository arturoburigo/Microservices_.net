using Microsoft.EntityFrameworkCore.Migrations;
using BCrypt.Net;

#nullable disable

namespace UsersMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Email", "Name", "Password", "Role" },
                values: new object[] {
                    "admin@admin.com",
                    "Admin",
                    BCrypt.Net.BCrypt.HashPassword("admin"),
                    "ADMIN"
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Email",
                keyValue: "admin@admin.com");
        }
    }
}