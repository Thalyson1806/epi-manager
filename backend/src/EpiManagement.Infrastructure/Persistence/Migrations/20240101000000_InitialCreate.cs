using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EpiManagement.Infrastructure.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sectors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_sectors", x => x.id));

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_users", x => x.id));

            migrationBuilder.CreateTable(
                name: "epis",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    validity_days = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_epis", x => x.id));

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cpf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    registration = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    admission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    biometric_template = table.Column<byte[]>(type: "bytea", nullable: true),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.id);
                    table.ForeignKey(name: "FK_employees_sectors_sector_id", column: x => x.sector_id, principalTable: "sectors", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sector_epis",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    epi_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    replacement_period_days = table.Column<int>(type: "integer", nullable: false),
                    max_quantity_allowed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sector_epis", x => x.id);
                    table.ForeignKey(name: "FK_sector_epis_sectors_sector_id", column: x => x.sector_id, principalTable: "sectors", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_sector_epis_epis_epi_id", column: x => x.epi_id, principalTable: "epis", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "epi_deliveries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    biometric_signature = table.Column<byte[]>(type: "bytea", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_epi_deliveries", x => x.id);
                    table.ForeignKey(name: "FK_epi_deliveries_employees_employee_id", column: x => x.employee_id, principalTable: "employees", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_epi_deliveries_users_operator_id", column: x => x.operator_id, principalTable: "users", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "epi_delivery_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    epi_delivery_id = table.Column<Guid>(type: "uuid", nullable: false),
                    epi_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    next_replacement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_epi_delivery_items", x => x.id);
                    table.ForeignKey(name: "FK_epi_delivery_items_epi_deliveries_epi_delivery_id", column: x => x.epi_delivery_id, principalTable: "epi_deliveries", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_epi_delivery_items_epis_epi_id", column: x => x.epi_id, principalTable: "epis", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    entity_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    old_values = table.Column<string>(type: "text", nullable: true),
                    new_values = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_audit_logs", x => x.id));

            // Indexes
            migrationBuilder.CreateIndex(name: "IX_employees_cpf", table: "employees", column: "cpf", unique: true);
            migrationBuilder.CreateIndex(name: "IX_employees_registration", table: "employees", column: "registration", unique: true);
            migrationBuilder.CreateIndex(name: "IX_employees_sector_id", table: "employees", column: "sector_id");
            migrationBuilder.CreateIndex(name: "IX_epis_code", table: "epis", column: "code", unique: true);
            migrationBuilder.CreateIndex(name: "IX_users_email", table: "users", column: "email", unique: true);
            migrationBuilder.CreateIndex(name: "IX_epi_deliveries_employee_id", table: "epi_deliveries", column: "employee_id");
            migrationBuilder.CreateIndex(name: "IX_epi_deliveries_operator_id", table: "epi_deliveries", column: "operator_id");
            migrationBuilder.CreateIndex(name: "IX_epi_delivery_items_epi_delivery_id", table: "epi_delivery_items", column: "epi_delivery_id");
            migrationBuilder.CreateIndex(name: "IX_sector_epis_sector_id", table: "sector_epis", column: "sector_id");
            migrationBuilder.CreateIndex(name: "IX_sector_epis_epi_id", table: "sector_epis", column: "epi_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "audit_logs");
            migrationBuilder.DropTable(name: "epi_delivery_items");
            migrationBuilder.DropTable(name: "epi_deliveries");
            migrationBuilder.DropTable(name: "sector_epis");
            migrationBuilder.DropTable(name: "employees");
            migrationBuilder.DropTable(name: "epis");
            migrationBuilder.DropTable(name: "sectors");
            migrationBuilder.DropTable(name: "users");
        }
    }
}
