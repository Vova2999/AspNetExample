using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspNetExample.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddInternProfessorIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Professors_DoctorId",
                table: "Professors");

            migrationBuilder.DropIndex(
                name: "IX_Interns_DoctorId",
                table: "Interns");

            migrationBuilder.CreateIndex(
                name: "IX_Professors_DoctorId",
                table: "Professors",
                column: "DoctorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interns_DoctorId",
                table: "Interns",
                column: "DoctorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Professors_DoctorId",
                table: "Professors");

            migrationBuilder.DropIndex(
                name: "IX_Interns_DoctorId",
                table: "Interns");

            migrationBuilder.CreateIndex(
                name: "IX_Professors_DoctorId",
                table: "Professors",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Interns_DoctorId",
                table: "Interns",
                column: "DoctorId");
        }
    }
}
