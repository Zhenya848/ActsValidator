using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "citext",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}
