using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeAuditUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "created_by_id",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_id",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "inactivated_by_id",
                table: "employees",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "inactivated_by_id",
                table: "employees");
        }
    }
}
