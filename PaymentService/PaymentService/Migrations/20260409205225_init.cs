using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pack",
                table: "products");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "payment_sessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "payment_sessions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "payment_sessions");

            migrationBuilder.DropColumn(
                name: "status",
                table: "payment_sessions");

            migrationBuilder.AddColumn<int>(
                name: "pack",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
