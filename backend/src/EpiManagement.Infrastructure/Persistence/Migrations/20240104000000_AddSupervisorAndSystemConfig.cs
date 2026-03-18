using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EpiManagement.Infrastructure.Persistence.Migrations
{
    public partial class AddSupervisorAndSystemConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "supervisor_name",
                table: "sectors",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "supervisor_email",
                table: "sectors",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "system_config",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    smtp_host = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    smtp_port = table.Column<int>(type: "integer", nullable: false),
                    smtp_user = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    smtp_password = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    smtp_from_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    smtp_from_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    smtp_use_ssl = table.Column<bool>(type: "boolean", nullable: false),
                    alert_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    alert_hour = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_config", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "supervisor_name", table: "sectors");
            migrationBuilder.DropColumn(name: "supervisor_email", table: "sectors");
            migrationBuilder.DropTable(name: "system_config");
        }
    }
}
