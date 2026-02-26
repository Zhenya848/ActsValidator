using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActsValidator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "collations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    act1 = table.Column<string>(type: "jsonb", nullable: true),
                    act2 = table.Column<string>(type: "jsonb", nullable: true),
                    ai_discrepancies = table.Column<string>(type: "jsonb", nullable: true),
                    discrepancies = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collations", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "collations");
        }
    }
}
