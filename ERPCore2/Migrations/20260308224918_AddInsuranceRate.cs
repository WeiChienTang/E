using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsuranceRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LaborInsuranceEmployeeRate = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    LaborInsuranceEmployerRate = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    HealthInsuranceEmployeeRate = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    HealthInsuranceEmployerRate = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    RetirementEmployerRate = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    MealTaxFreeLimit = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    TransportTaxFreeLimit = table.Column<decimal>(type: "decimal(10,0)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceRates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsuranceRates");
        }
    }
}
