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
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    act1name = table.Column<string>(type: "text", nullable: false),
                    act2name = table.Column<string>(type: "text", nullable: false),
                    coincidences_count = table.Column<int>(type: "integer", nullable: false),
                    rows_processed = table.Column<int>(type: "integer", nullable: false),
                    collation_errors = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
