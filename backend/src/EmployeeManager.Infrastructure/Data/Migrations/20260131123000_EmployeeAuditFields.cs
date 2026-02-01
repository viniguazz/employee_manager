using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "employees",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "employees",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "employees",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deactivated_at",
                table: "employees",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "deactivated_at",
                table: "employees");
        }
    }
}
