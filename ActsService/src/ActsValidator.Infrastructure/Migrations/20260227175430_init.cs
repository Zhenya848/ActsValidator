using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActsValidator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "act1",
                table: "collations");

            migrationBuilder.DropColumn(
                name: "act2",
                table: "collations");

            migrationBuilder.DropColumn(
                name: "ai_discrepancies",
                table: "collations");

            migrationBuilder.AddColumn<string>(
                name: "act1name",
                table: "collations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "act2name",
                table: "collations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "collations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ai_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    collation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    discrepancies = table.Column<string>(type: "jsonb", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_ai_requests_collations_collation_id",
                        column: x => x.collation_id,
                        principalTable: "collations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ai_requests_collation_id",
                table: "ai_requests",
                column: "collation_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_requests");

            migrationBuilder.DropColumn(
                name: "act1name",
                table: "collations");

            migrationBuilder.DropColumn(
                name: "act2name",
                table: "collations");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "collations");

            migrationBuilder.AddColumn<string>(
                name: "act1",
                table: "collations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "act2",
                table: "collations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ai_discrepancies",
                table: "collations",
                type: "jsonb",
                nullable: true);
        }
    }
}
