using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace User.Management.API.Migrations
{
    /// <inheritdoc />
    public partial class RoleSeeded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3b098b51-725c-491c-85c5-e937c8ad6865", "1", "Admin", "ADMIN" },
                    { "83f8b40e-19e2-43b1-9d46-0dc0ac9205fb", "3", "HR", "HR" },
                    { "cd7d6b74-0fd3-4fc0-9d5d-61d2154d622d", "2", "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3b098b51-725c-491c-85c5-e937c8ad6865");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "83f8b40e-19e2-43b1-9d46-0dc0ac9205fb");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cd7d6b74-0fd3-4fc0-9d5d-61d2154d622d");
        }
    }
}
