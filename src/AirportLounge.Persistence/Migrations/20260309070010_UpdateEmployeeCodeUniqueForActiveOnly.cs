using Microsoft.EntityFrameworkCore.Migrations;

namespace AirportLounge.Persistence.Migrations;

public partial class UpdateEmployeeCodeUniqueForActiveOnly : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_employees_EmployeeCode",
            table: "employees");

        migrationBuilder.CreateIndex(
            name: "IX_employees_EmployeeCode",
            table: "employees",
            column: "EmployeeCode",
            unique: true,
            filter: "\"IsDeleted\" = FALSE");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_employees_EmployeeCode",
            table: "employees");

        migrationBuilder.CreateIndex(
            name: "IX_employees_EmployeeCode",
            table: "employees",
            column: "EmployeeCode",
            unique: true);
    }
}

