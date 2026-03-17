using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EpiManagement.Infrastructure.Persistence.Migrations
{
    public partial class AddWorkShift : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "work_shift",
                table: "employees",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "work_shift", table: "employees");
        }
    }
}
