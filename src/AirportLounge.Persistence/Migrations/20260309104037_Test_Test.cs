using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirportLounge.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Test_Test : Migration
    {
        /// <inheritdoc />
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
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
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
}
