using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddGovernmentAgency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GovernmentAgencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AgencyCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Jurisdiction = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SupervisorAgency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AgencyType = table.Column<int>(type: "int", nullable: true),
                    AgencyStatus = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDraft = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernmentAgencies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GovernmentAgencies_Code",
                table: "GovernmentAgencies",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GovernmentAgencies");
        }
    }
}
