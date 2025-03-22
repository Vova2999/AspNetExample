using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspNetExample.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initialize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Building = table.Column<int>(type: "int", nullable: false),
                    Financing = table.Column<decimal>(type: "money", nullable: false, defaultValue: 0m),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.CheckConstraint("CK_Departments_Building", "Building BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_Departments_Financing", "Financing >= 0");
                    table.CheckConstraint("CK_Departments_Name", "LEN(Name) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Diseases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diseases", x => x.Id);
                    table.CheckConstraint("CK_Diseases_Name", "LEN(Name) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salary = table.Column<decimal>(type: "money", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.CheckConstraint("CK_Doctors_Name", "LEN(Name) > 0");
                    table.CheckConstraint("CK_Doctors_Salary", "Salary > 0");
                    table.CheckConstraint("CK_Doctors_Surname", "LEN(Surname) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Examinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examinations", x => x.Id);
                    table.CheckConstraint("CK_Examinations_Name", "LEN(Name) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Wards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Places = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wards", x => x.Id);
                    table.CheckConstraint("CK_Wards_Name", "LEN(Name) > 0");
                    table.CheckConstraint("CK_Wards_Places", "Places > 0");
                    table.ForeignKey(
                        name: "FK_Wards_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interns_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Professors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Professors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Professors_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorsExaminations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "GETDATE()"),
                    DiseaseId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    ExaminationId = table.Column<int>(type: "int", nullable: false),
                    WardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorsExaminations", x => x.Id);
                    table.CheckConstraint("CK_DoctorsExaminations_Date", "Date <= GETDATE()");
                    table.ForeignKey(
                        name: "FK_DoctorsExaminations_Diseases_DiseaseId",
                        column: x => x.DiseaseId,
                        principalTable: "Diseases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorsExaminations_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorsExaminations_Examinations_ExaminationId",
                        column: x => x.ExaminationId,
                        principalTable: "Examinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorsExaminations_Wards_WardId",
                        column: x => x.WardId,
                        principalTable: "Wards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diseases_Name",
                table: "Diseases",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorsExaminations_DiseaseId",
                table: "DoctorsExaminations",
                column: "DiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorsExaminations_DoctorId",
                table: "DoctorsExaminations",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorsExaminations_ExaminationId",
                table: "DoctorsExaminations",
                column: "ExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorsExaminations_WardId",
                table: "DoctorsExaminations",
                column: "WardId");

            migrationBuilder.CreateIndex(
                name: "IX_Examinations_Name",
                table: "Examinations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interns_DoctorId",
                table: "Interns",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Professors_DoctorId",
                table: "Professors",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Wards_DepartmentId",
                table: "Wards",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Wards_Name",
                table: "Wards",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorsExaminations");

            migrationBuilder.DropTable(
                name: "Interns");

            migrationBuilder.DropTable(
                name: "Professors");

            migrationBuilder.DropTable(
                name: "Diseases");

            migrationBuilder.DropTable(
                name: "Examinations");

            migrationBuilder.DropTable(
                name: "Wards");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
