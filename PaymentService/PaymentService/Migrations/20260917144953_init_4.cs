using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Migrations
{
    /// <inheritdoc />
    public partial class init_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "product_name",
                table: "receipts");

            migrationBuilder.DropColumn(
                name: "tax_receipt_status",
                table: "receipts");

            migrationBuilder.AddColumn<string>(
                name: "print_url",
                table: "receipts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "print_url",
                table: "receipts");

            migrationBuilder.AddColumn<string>(
                name: "product_name",
                table: "receipts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tax_receipt_status",
                table: "receipts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
