using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirportLounge.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LeaveWorkflowStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DecisionReason",
                table: "leave_requests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHalfDay",
                table: "leave_requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "leave_requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "leave_requests",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservedDays",
                table: "leave_balances",
                type: "numeric(5,1)",
                precision: 5,
                scale: 1,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "leave_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_leave_attachments_leave_requests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "leave_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_leave_attachments_LeaveRequestId",
                table: "leave_attachments",
                column: "LeaveRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "leave_attachments");

            migrationBuilder.DropColumn(
                name: "DecisionReason",
                table: "leave_requests");

            migrationBuilder.DropColumn(
                name: "IsHalfDay",
                table: "leave_requests");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "leave_requests");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "leave_requests");

            migrationBuilder.DropColumn(
                name: "ReservedDays",
                table: "leave_balances");
        }
    }
}
