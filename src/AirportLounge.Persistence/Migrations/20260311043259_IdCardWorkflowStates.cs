using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirportLounge.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IdCardWorkflowStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "IssuedById",
                table: "employee_id_cards",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "IssuedAt",
                table: "employee_id_cards",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedAt",
                table: "employee_id_cards",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "employee_id_cards",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdCardTemplateId",
                table: "employee_id_cards",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacedByCardId",
                table: "employee_id_cards",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "employee_id_cards",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "id_card_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CardId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: false),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PerformedById = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_id_card_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_id_card_events_employee_id_cards_CardId",
                        column: x => x.CardId,
                        principalTable: "employee_id_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_id_card_events_users_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "id_card_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LayoutJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_id_card_templates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_id_cards_IdCardTemplateId",
                table: "employee_id_cards",
                column: "IdCardTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_id_cards_ReplacedByCardId",
                table: "employee_id_cards",
                column: "ReplacedByCardId");

            migrationBuilder.CreateIndex(
                name: "IX_id_card_events_CardId",
                table: "id_card_events",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_id_card_events_PerformedById",
                table: "id_card_events",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_id_card_templates_Name",
                table: "id_card_templates",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_id_cards_employee_id_cards_ReplacedByCardId",
                table: "employee_id_cards",
                column: "ReplacedByCardId",
                principalTable: "employee_id_cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_id_cards_id_card_templates_IdCardTemplateId",
                table: "employee_id_cards",
                column: "IdCardTemplateId",
                principalTable: "id_card_templates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employee_id_cards_employee_id_cards_ReplacedByCardId",
                table: "employee_id_cards");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_id_cards_id_card_templates_IdCardTemplateId",
                table: "employee_id_cards");

            migrationBuilder.DropTable(
                name: "id_card_events");

            migrationBuilder.DropTable(
                name: "id_card_templates");

            migrationBuilder.DropIndex(
                name: "IX_employee_id_cards_IdCardTemplateId",
                table: "employee_id_cards");

            migrationBuilder.DropIndex(
                name: "IX_employee_id_cards_ReplacedByCardId",
                table: "employee_id_cards");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                table: "employee_id_cards");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "employee_id_cards");

            migrationBuilder.DropColumn(
                name: "IdCardTemplateId",
                table: "employee_id_cards");

            migrationBuilder.DropColumn(
                name: "ReplacedByCardId",
                table: "employee_id_cards");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "employee_id_cards");

            migrationBuilder.AlterColumn<Guid>(
                name: "IssuedById",
                table: "employee_id_cards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "IssuedAt",
                table: "employee_id_cards",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
