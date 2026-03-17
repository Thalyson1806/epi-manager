using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EpiManagement.Infrastructure.Persistence.Migrations
{
    public partial class AddEarlyReplacement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_early_replacement",
                table: "epi_delivery_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "early_replacement_reason",
                table: "epi_delivery_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "is_early_replacement", table: "epi_delivery_items");
            migrationBuilder.DropColumn(name: "early_replacement_reason", table: "epi_delivery_items");
        }
    }
}
