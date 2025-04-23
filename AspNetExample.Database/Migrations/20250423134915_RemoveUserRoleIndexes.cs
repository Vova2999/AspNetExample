using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspNetExample.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserRoleIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");
        }
    }
}
