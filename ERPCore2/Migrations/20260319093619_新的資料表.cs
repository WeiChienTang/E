using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 新的資料表 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SwiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_Banks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    ContactPersonName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_BusinessCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Colors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_Colors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Representative = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShortNameEn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstablishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CapitalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    InvoiceTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyModules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CompanyModules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompositionCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_CompositionCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsBaseCurrency = table.Column<bool>(type: "bit", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeletedRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecordId = table.Column<int>(type: "int", nullable: false),
                    RecordDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeleteReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_DeletedRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultAccessLevel = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DocumentCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_EmployeePositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ErrorId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExceptionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InnerException = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthInsuranceGrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    SalaryFrom = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    SalaryTo = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    InsuredAmount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthInsuranceGrades", x => x.Id);
                });

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
                    OvertimeRate1 = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    OvertimeRate2 = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    RestDayRate1 = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    RestDayRate2 = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    NationalHolidayRate = table.Column<decimal>(type: "decimal(6,4)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    AffectsCost = table.Column<bool>(type: "bit", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    AutoGenerateNumber = table.Column<bool>(type: "bit", nullable: false),
                    NumberPrefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_InventoryTransactionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSaleable = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ItemCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LaborInsuranceGrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    SalaryFrom = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    SalaryTo = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    InsuredSalary = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaborInsuranceGrades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MinimumWages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    HourlyAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinimumWages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaperSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Width = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Height = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    TopMargin = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    BottomMargin = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    LeftMargin = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    RightMargin = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    Orientation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("PK_PaperSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    IsSystemItem = table.Column<bool>(type: "bit", nullable: false),
                    IsTaxable = table.Column<bool>(type: "bit", nullable: false),
                    IsInsuranceBasis = table.Column<bool>(type: "bit", nullable: false),
                    IsRetirementBasis = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PayrollItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    PeriodStatus = table.Column<int>(type: "int", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_PayrollPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrepaymentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_PrepaymentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_PurchaseReturnReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_SalesReturnReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_Sizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    EnableQuotationApproval = table.Column<bool>(type: "bit", nullable: false),
                    EnablePurchaseOrderApproval = table.Column<bool>(type: "bit", nullable: false),
                    EnablePurchaseReceivingApproval = table.Column<bool>(type: "bit", nullable: false),
                    EnablePurchaseReturnApproval = table.Column<bool>(type: "bit", nullable: false),
                    EnableSalesOrderApproval = table.Column<bool>(type: "bit", nullable: false),
                    EnableSalesReturnApproval = table.Column<bool>(type: "bit", nullable: false),
                    EnableSalesDeliveryApproval = table.Column<bool>(type: "bit", nullable: false),
                    EnableInventoryTransferApproval = table.Column<bool>(type: "bit", nullable: false),
                    HideApprovalInfoSection = table.Column<bool>(type: "bit", nullable: false),
                    AutoCreateCustomerSubAccount = table.Column<bool>(type: "bit", nullable: false),
                    AutoCreateSupplierSubAccount = table.Column<bool>(type: "bit", nullable: false),
                    AutoCreateItemSubAccount = table.Column<bool>(type: "bit", nullable: false),
                    CustomerSubAccountParentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerNoteSubAccountParentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerReturnSubAccountParentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerAdvanceSubAccountParentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SupplierSubAccountParentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SupplierNoteSubAccountParentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SupplierReturnSubAccountParentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SupplierAdvanceSubAccountParentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemSubAccountParentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubAccountCodeFormat = table.Column<int>(type: "int", nullable: false),
                    PayrollPayDay = table.Column<int>(type: "int", nullable: false),
                    PayrollCutoffDay = table.Column<int>(type: "int", nullable: false),
                    SalaryMonthDivisor = table.Column<int>(type: "int", nullable: false),
                    OvertimeRoundingUnit = table.Column<int>(type: "int", nullable: false),
                    LateTolerance = table.Column<int>(type: "int", nullable: false),
                    PayrollExpenseAccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OvertimeExpenseAccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LaborInsuranceExpenseAccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HealthInsuranceExpenseAccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RetirementExpenseAccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccruedPayrollAccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WithholdingLaborInsuranceAccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WithholdingHealthInsuranceAccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WithholdingTaxAccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    VoluntaryRetirementAccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_SystemParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TextMessageTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HeaderText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FooterText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DetailFormatJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TextMessageTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EnglishName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_Units", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_VehicleTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Weathers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_Weathers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WithholdingTaxTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaryFrom = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    SalaryTo = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    DependentCount = table.Column<int>(type: "int", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithholdingTaxTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyBankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_CompanyBankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyBankAccounts_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyBankAccounts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FiscalPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    PeriodNumber = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodStatus = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    ReopenReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReopenedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_FiscalPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiscalPeriods_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryType = table.Column<int>(type: "int", nullable: false),
                    JournalEntryStatus = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    FiscalPeriod = table.Column<int>(type: "int", nullable: false),
                    SourceDocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SourceDocumentId = table.Column<int>(type: "int", nullable: true),
                    SourceDocumentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TotalDebitAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCreditAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsReversed = table.Column<bool>(type: "bit", nullable: false),
                    ReversalEntryId = table.Column<int>(type: "int", nullable: true),
                    ReversedEntryId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_JournalEntries_ReversalEntryId",
                        column: x => x.ReversalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_JournalEntries_ReversedEntryId",
                        column: x => x.ReversedEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SetoffDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetoffType = table.Column<int>(type: "int", nullable: false),
                    SetoffDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RelatedPartyId = table.Column<int>(type: "int", nullable: false),
                    RelatedPartyType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TotalSetoffAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentSetoffAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCollectionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAllowanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentPrepaymentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrepaymentSetoffAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsJournalized = table.Column<bool>(type: "bit", nullable: false),
                    JournalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_SetoffDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetoffDocuments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DocumentCategoryId = table.Column<int>(type: "int", nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccessLevel = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_DocumentCategories_DocumentCategoryId",
                        column: x => x.DocumentCategoryId,
                        principalTable: "DocumentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportPrintConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaperSettingId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ReportPrintConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportPrintConfigurations_PaperSettings_PaperSettingId",
                        column: x => x.PaperSettingId,
                        principalTable: "PaperSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    ResponsiblePerson = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupplierContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MobilePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupplierAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentDay = table.Column<int>(type: "int", nullable: true),
                    InvoiceTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentDays = table.Column<int>(type: "int", nullable: false),
                    CurrentPayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupplierStatus = table.Column<int>(type: "int", nullable: false),
                    SupplierType = table.Column<int>(type: "int", nullable: true),
                    BillingAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suppliers_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    ProductionUnitId = table.Column<int>(type: "int", nullable: true),
                    ProductionUnitConversionRate = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    SizeId = table.Column<int>(type: "int", nullable: true),
                    ItemCategoryId = table.Column<int>(type: "int", nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    StandardCost = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
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
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_ItemCategories_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalTable: "ItemCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Items_Sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Sizes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_Units_ProductionUnitId",
                        column: x => x.ProductionUnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitConversions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversionRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FromUnitId = table.Column<int>(type: "int", nullable: false),
                    ToUnitId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_UnitConversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitConversions_Units_FromUnitId",
                        column: x => x.FromUnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnitConversions_Units_ToUnitId",
                        column: x => x.ToUnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Zone = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Aisle = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Level = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaxCapacity = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_WarehouseLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseLocations_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetoffPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetoffDocumentId = table.Column<int>(type: "int", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: true),
                    ReceivedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AllowanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CheckNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_SetoffPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetoffPayments_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SetoffPayments_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SetoffPayments_SetoffDocuments_SetoffDocumentId",
                        column: x => x.SetoffDocumentId,
                        principalTable: "SetoffDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DocumentFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentFiles_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierBankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_SupplierBankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierBankAccounts_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierBankAccounts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryStocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_InventoryStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryStocks_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ItemPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemPhotos_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemSuppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    IsPreferred = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    LastPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastPurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupplierItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ItemSuppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemSuppliers_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemSuppliers_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScaleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ScaleTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScaleTypes_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SetoffItemDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetoffDocumentId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SourceDetailType = table.Column<int>(type: "int", nullable: false),
                    SourceDetailId = table.Column<int>(type: "int", nullable: false),
                    CurrentSetoffAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalSetoffAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentAllowanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAllowanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_SetoffItemDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetoffItemDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetoffItemDetails_SetoffDocuments_SetoffDocumentId",
                        column: x => x.SetoffDocumentId,
                        principalTable: "SetoffDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierPricings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SupplierItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MinOrderQuantity = table.Column<int>(type: "int", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PurchaseRemarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_SupplierPricings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPricings_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierPricings_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryStockDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrentStock = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReservedStock = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InTransitStock = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InProductionStock = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AverageCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    LastTransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MinStockLevel = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MaxStockLevel = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InventoryStockId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_InventoryStockDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryStockDetails_InventoryStocks_InventoryStockId",
                        column: x => x.InventoryStockId,
                        principalTable: "InventoryStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryStockDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryStockDetails_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReservationType = table.Column<int>(type: "int", nullable: false),
                    ReservationStatus = table.Column<int>(type: "int", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReservedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReleasedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReservationRemarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    InventoryStockId = table.Column<int>(type: "int", nullable: true),
                    InventoryStockDetailId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_InventoryReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReservations_InventoryStockDetails_InventoryStockDetailId",
                        column: x => x.InventoryStockDetailId,
                        principalTable: "InventoryStockDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryReservations_InventoryStocks_InventoryStockId",
                        column: x => x.InventoryStockId,
                        principalTable: "InventoryStocks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryReservations_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryReservations_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryReservations_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnglishDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccountLevel = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    IsDetailAccount = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    LinkedCustomerId = table.Column<int>(type: "int", nullable: true),
                    LinkedSupplierId = table.Column<int>(type: "int", nullable: true),
                    LinkedItemId = table.Column<int>(type: "int", nullable: true),
                    IsAutoGenerated = table.Column<bool>(type: "bit", nullable: false),
                    SubAccountLinkType = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_AccountItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountItems_AccountItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AccountItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountItems_Items_LinkedItemId",
                        column: x => x.LinkedItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AccountItems_Suppliers_LinkedSupplierId",
                        column: x => x.LinkedSupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalEntryId = table.Column<int>(type: "int", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    AccountItemId = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_JournalEntryLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_AccountItems_AccountItemId",
                        column: x => x.AccountItemId,
                        principalTable: "AccountItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EventTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    Color = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    SourceModule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    ReminderMinutes = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerBankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_CustomerBankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerBankAccounts_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerComplaints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ComplaintDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ComplaintStatus = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_CustomerComplaints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompanyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    ResponsiblePerson = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MobilePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShippingAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentDate = table.Column<int>(type: "int", nullable: true),
                    InvoiceTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentDays = table.Column<int>(type: "int", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    CustomerStatus = table.Column<int>(type: "int", nullable: false),
                    CustomerType = table.Column<int>(type: "int", nullable: true),
                    CustomerSource = table.Column<int>(type: "int", nullable: true),
                    CreditRating = table.Column<int>(type: "int", nullable: true),
                    DefaultDiscountRate = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    BillingAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ItemCustomers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    IsPreferred = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CustomerPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiscountRate = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    CustomerItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastSalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastSaleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ItemCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemCustomers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemCustomers_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PriceType = table.Column<int>(type: "int", nullable: false),
                    OldPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedCustomerId = table.Column<int>(type: "int", nullable: true),
                    RelatedSupplierId = table.Column<int>(type: "int", nullable: true),
                    ChangeDetails = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
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
                    table.PrimaryKey("PK_PriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceHistories_Customers_RelatedCustomerId",
                        column: x => x.RelatedCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PriceHistories_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PriceHistories_Suppliers_RelatedSupplierId",
                        column: x => x.RelatedSupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SetoffPrepayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrepaymentTypeId = table.Column<int>(type: "int", nullable: false),
                    SourceDocumentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UsedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    SetoffDocumentId = table.Column<int>(type: "int", nullable: true),
                    SourcePrepaymentId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SetoffPrepayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetoffPrepayments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetoffPrepayments_PrepaymentTypes_PrepaymentTypeId",
                        column: x => x.PrepaymentTypeId,
                        principalTable: "PrepaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SetoffPrepayments_SetoffDocuments_SetoffDocumentId",
                        column: x => x.SetoffDocumentId,
                        principalTable: "SetoffDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SetoffPrepayments_SetoffPrepayments_SourcePrepaymentId",
                        column: x => x.SourcePrepaymentId,
                        principalTable: "SetoffPrepayments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SetoffPrepayments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SetoffPrepaymentUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetoffPrepaymentId = table.Column<int>(type: "int", nullable: false),
                    SetoffDocumentId = table.Column<int>(type: "int", nullable: false),
                    UsedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UsageDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourcePrepaymentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_SetoffPrepaymentUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetoffPrepaymentUsages_SetoffDocuments_SetoffDocumentId",
                        column: x => x.SetoffDocumentId,
                        principalTable: "SetoffDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetoffPrepaymentUsages_SetoffPrepayments_SetoffPrepaymentId",
                        column: x => x.SetoffPrepaymentId,
                        principalTable: "SetoffPrepayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerVisits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VisitType = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NextFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_CustomerVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerVisits_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ParentDepartmentId = table.Column<int>(type: "int", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    DeputyManagerId = table.Column<int>(type: "int", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    IsSystemUser = table.Column<bool>(type: "bit", nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "bit", nullable: false),
                    Account = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    EmployeePositionId = table.Column<int>(type: "int", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnglishName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaritalStatus = table.Column<int>(type: "int", nullable: true),
                    BloodType = table.Column<int>(type: "int", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HomeAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MailingAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmergencyContact = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmergencyPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactRelationship = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmployeeType = table.Column<int>(type: "int", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResignationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmploymentStatus = table.Column<int>(type: "int", nullable: false),
                    HasInitializedDashboard = table.Column<bool>(type: "bit", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Employees_EmployeePositions_EmployeePositionId",
                        column: x => x.EmployeePositionId,
                        principalTable: "EmployeePositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Employees_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeBankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_EmployeeBankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeBankAccounts_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeBankAccounts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDashboardPanels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_EmployeeDashboardPanels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDashboardPanels_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLicenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LicenseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IssuingAuthority = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlertDays = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EmployeeLicenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeLicenses_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false),
                    Zoom = table.Column<int>(type: "int", nullable: false),
                    Theme = table.Column<int>(type: "int", nullable: false),
                    ShortcutPageSearch = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShortcutReportSearch = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShortcutStickyNotes = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShortcutCalendar = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShortcutQuickAction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EnableCalendar = table.Column<bool>(type: "bit", nullable: false),
                    EnableStickyNote = table.Column<bool>(type: "bit", nullable: false),
                    ShowCalendarBadge = table.Column<bool>(type: "bit", nullable: false),
                    ShowNoteBadge = table.Column<bool>(type: "bit", nullable: false),
                    DefaultReminderMinutes = table.Column<int>(type: "int", nullable: false),
                    ToastSuccessDurationMs = table.Column<int>(type: "int", nullable: false),
                    ToastErrorDurationMs = table.Column<int>(type: "int", nullable: false),
                    ToastWarningDurationMs = table.Column<int>(type: "int", nullable: false),
                    ToastInfoDurationMs = table.Column<int>(type: "int", nullable: false),
                    ShowDisabledModules = table.Column<bool>(type: "bit", nullable: false),
                    DefaultPageSize = table.Column<int>(type: "int", nullable: false),
                    ShowUnsavedChangesWarning = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_EmployeePreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePreferences_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSalaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    SalaryType = table.Column<int>(type: "int", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    LaborInsuredSalary = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    HealthInsuredAmount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    DependentCount = table.Column<int>(type: "int", nullable: false),
                    TaxType = table.Column<int>(type: "int", nullable: false),
                    VoluntaryRetirementRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeSalaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeSalaries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ToolName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToolCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeTools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeTools_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTrainingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrainingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrainingHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrainingOrganization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_EmployeeTrainingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeTrainingRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceDocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceDocumentId = table.Column<int>(type: "int", nullable: true),
                    TotalQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    InventoryStockId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_InventoryStocks_InventoryStockId",
                        column: x => x.InventoryStockId,
                        principalTable: "InventoryStocks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemCompositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ParentItemId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    CompositionCategoryId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ItemCompositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemCompositions_CompositionCategories_CompositionCategoryId",
                        column: x => x.CompositionCategoryId,
                        principalTable: "CompositionCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ItemCompositions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItemCompositions_Employees_CreatedByEmployeeId",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItemCompositions_Items_ParentItemId",
                        column: x => x.ParentItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyAttendanceSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    ScheduledWorkDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    ActualWorkDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    AbsentDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    SickLeaveDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    OvertimeHours1 = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    OvertimeHours2 = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    HolidayOvertimeHours = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    NationalHolidayHours = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyAttendanceSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyAttendanceSummaries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ScheduledWorkDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    ActualWorkDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    AbsentDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    SickLeaveDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    OvertimeHours1 = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    OvertimeHours2 = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    HolidayOvertimeHours = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    NationalHolidayHours = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    GrossIncome = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    TotalDeduction = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    NetPay = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    LaborInsuranceSalary = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    HealthInsuranceAmount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    TaxableIncome = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    WithholdingTax = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    EmployerLaborInsurance = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    EmployerHealthInsurance = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    EmployerRetirement = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    RecordStatus = table.Column<int>(type: "int", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CalculatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_PayrollRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollRecords_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TaxCalculationMethod = table.Column<int>(type: "int", nullable: false),
                    PurchasePersonnel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchaseTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Employees_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReceivings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchaseReceivingTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxCalculationMethod = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsJournalized = table.Column<bool>(type: "bit", nullable: false),
                    JournalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseReceivings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReceivings_Employees_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseReceivings_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaxCalculationMethod = table.Column<int>(type: "int", nullable: false),
                    TotalReturnAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsJournalized = table.Column<bool>(type: "bit", nullable: false),
                    JournalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    ReturnReasonId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturns_Employees_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseReturns_PurchaseReturnReasons_ReturnReasonId",
                        column: x => x.ReturnReasonId,
                        principalTable: "PurchaseReturnReasons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseReturns_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubtotalBeforeDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuotationTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxCalculationMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentTerms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    SalespersonId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Quotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Quotations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotations_Employees_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Quotations_Employees_SalespersonId",
                        column: x => x.SalespersonId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesDeliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxCalculationMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentTerms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShippingMethod = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsShipped = table.Column<bool>(type: "bit", nullable: false),
                    ActualShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    RejectReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsJournalized = table.Column<bool>(type: "bit", nullable: false),
                    JournalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    SalespersonId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SalesDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_Employees_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_Employees_SalespersonId",
                        column: x => x.SalespersonId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesDeliveries_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxCalculationMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentTerms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    SalespersonId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SalesOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Employees_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesOrders_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesOrders_Employees_SalespersonId",
                        column: x => x.SalespersonId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesReturns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxCalculationMethod = table.Column<int>(type: "int", nullable: false),
                    TotalReturnAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsRefunded = table.Column<bool>(type: "bit", nullable: false),
                    RefundDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsJournalized = table.Column<bool>(type: "bit", nullable: false),
                    JournalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    ReturnReasonId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SalesReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturns_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturns_Employees_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesReturns_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesReturns_SalesReturnReasons_ReturnReasonId",
                        column: x => x.ReturnReasonId,
                        principalTable: "SalesReturnReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesTargets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: true),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalespersonId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SalesTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesTargets_Employees_SalespersonId",
                        column: x => x.SalespersonId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StickyNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Color = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_StickyNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StickyNotes_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTakings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TakingNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TakingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    TakingType = table.Column<int>(type: "int", nullable: false),
                    TakingStatus = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TakingPersonnel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupervisingPersonnel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAdjustmentGenerated = table.Column<bool>(type: "bit", nullable: false),
                    TotalItems = table.Column<int>(type: "int", nullable: false),
                    CompletedItems = table.Column<int>(type: "int", nullable: false),
                    DifferenceItems = table.Column<int>(type: "int", nullable: false),
                    DifferenceAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_StockTakings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTakings_Employees_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockTakings_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockTakings_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplierVisits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VisitType = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NextFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SupplierVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierVisits_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierVisits_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicensePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OwnershipType = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ManufactureYear = table.Column<int>(type: "int", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EngineNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ChassisNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FuelType = table.Column<int>(type: "int", nullable: true),
                    LoadCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Mileage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InsuranceExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InsuranceCompany = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InsurancePolicyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InspectionExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceMileage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VehicleTypeId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vehicles_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vehicles_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vehicles_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vehicles_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDashboardConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PanelId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    NavigationItemKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    WidgetSettings = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeDashboardConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDashboardConfigs_EmployeeDashboardPanels_PanelId",
                        column: x => x.PanelId,
                        principalTable: "EmployeeDashboardPanels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeDashboardConfigs_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSalaryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeSalaryId = table.Column<int>(type: "int", nullable: false),
                    PayrollItemId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSalaryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeSalaryItems_EmployeeSalaries_EmployeeSalaryId",
                        column: x => x.EmployeeSalaryId,
                        principalTable: "EmployeeSalaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeSalaryItems_PayrollItems_PayrollItemId",
                        column: x => x.PayrollItemId,
                        principalTable: "PayrollItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryTransactionId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    StockBefore = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    StockAfter = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    OperationNote = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OperationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceDetailId = table.Column<int>(type: "int", nullable: true),
                    InventoryStockId = table.Column<int>(type: "int", nullable: true),
                    InventoryStockDetailId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_InventoryTransactionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactionDetails_InventoryStockDetails_InventoryStockDetailId",
                        column: x => x.InventoryStockDetailId,
                        principalTable: "InventoryStockDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryTransactionDetails_InventoryStocks_InventoryStockId",
                        column: x => x.InventoryStockId,
                        principalTable: "InventoryStocks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryTransactionDetails_InventoryTransactions_InventoryTransactionId",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTransactionDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransactionDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItemCompositionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCompositionId = table.Column<int>(type: "int", nullable: false),
                    ComponentItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    ComponentCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("PK_ItemCompositionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemCompositionDetails_ItemCompositions_ItemCompositionId",
                        column: x => x.ItemCompositionId,
                        principalTable: "ItemCompositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemCompositionDetails_Items_ComponentItemId",
                        column: x => x.ComponentItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemCompositionDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRecordDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollRecordId = table.Column<int>(type: "int", nullable: false),
                    PayrollItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    UnitAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRecordDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollRecordDetails_PayrollItems_PayrollItemId",
                        column: x => x.PayrollItemId,
                        principalTable: "PayrollItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollRecordDetails_PayrollRecords_PayrollRecordId",
                        column: x => x.PayrollRecordId,
                        principalTable: "PayrollRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    OrderQuantity = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false),
                    ReturnedQuantity = table.Column<int>(type: "int", nullable: false),
                    IsReceivingCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ReceivedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseOrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_Employees_CompletedByEmployeeId",
                        column: x => x.CompletedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuotationDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConvertedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    SelectedCompositionId = table.Column<int>(type: "int", nullable: true),
                    QuotationId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_QuotationDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationDetails_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuotationDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "QuotationPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationId = table.Column<int>(type: "int", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_QuotationPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationPhotos_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduleStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    SourceDocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceDocumentId = table.Column<int>(type: "int", nullable: true),
                    SalesOrderId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ProductionSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionSchedules_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductionSchedules_Employees_CreatedByEmployeeId",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductionSchedules_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SalesOrderPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderPhotos_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTakingDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockTakingId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    SystemStock = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ActualStock = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    DetailStatus = table.Column<int>(type: "int", nullable: false),
                    TakingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TakingPersonnel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DetailRemarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsAdjusted = table.Column<bool>(type: "bit", nullable: false),
                    AdjustmentNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("PK_StockTakingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTakingDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTakingDetails_StockTakings_StockTakingId",
                        column: x => x.StockTakingId,
                        principalTable: "StockTakings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockTakingDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ScaleRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EntryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExitWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExitTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DisposalFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PurchaseFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ScaleRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScaleRecords_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ScaleRecords_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ScaleRecords_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScaleRecords_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScaleRecords_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VehicleMaintenances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    MaintenanceType = table.Column<int>(type: "int", nullable: false),
                    MaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MileageAtMaintenance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ServiceProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_VehicleMaintenances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleMaintenances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehicleMaintenances_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReceivingDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseReceivingId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    OrderQuantity = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsReceivingCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsSettled = table.Column<bool>(type: "bit", nullable: false),
                    TotalPaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalReturnedQuantity = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PurchaseReceivingDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReceivingDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReceivingDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                        column: x => x.PurchaseOrderDetailId,
                        principalTable: "PurchaseOrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseReceivingDetails_PurchaseReceivings_PurchaseReceivingId",
                        column: x => x.PurchaseReceivingId,
                        principalTable: "PurchaseReceivings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReceivingDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseReceivingDetails_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationCompositionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationDetailId = table.Column<int>(type: "int", nullable: false),
                    ComponentItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    ComponentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
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
                    table.PrimaryKey("PK_QuotationCompositionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationCompositionDetails_Items_ComponentItemId",
                        column: x => x.ComponentItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationCompositionDetails_QuotationDetails_QuotationDetailId",
                        column: x => x.QuotationDetailId,
                        principalTable: "QuotationDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuotationCompositionDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DeliveredQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    IsSettled = table.Column<bool>(type: "bit", nullable: false),
                    TotalReceivedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SelectedCompositionId = table.Column<int>(type: "int", nullable: true),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    QuotationDetailId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    ScheduledQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ProducedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
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
                    table.PrimaryKey("PK_SalesOrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_QuotationDetails_QuotationDetailId",
                        column: x => x.QuotationDetailId,
                        principalTable: "QuotationDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaterialIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    ProductionScheduleId = table.Column<int>(type: "int", nullable: true),
                    IssueType = table.Column<int>(type: "int", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    IsJournalized = table.Column<bool>(type: "bit", nullable: false),
                    JournalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_MaterialIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialIssues_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaterialIssues_Employees_ConfirmedByEmployeeId",
                        column: x => x.ConfirmedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaterialIssues_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaterialIssues_ProductionSchedules_ProductionScheduleId",
                        column: x => x.ProductionScheduleId,
                        principalTable: "ProductionSchedules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnQuantity = table.Column<int>(type: "int", nullable: false),
                    OriginalUnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSettled = table.Column<bool>(type: "bit", nullable: false),
                    TotalReceivedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchaseReturnId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    PurchaseReceivingDetailId = table.Column<int>(type: "int", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseReturnDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                        column: x => x.PurchaseOrderDetailId,
                        principalTable: "PurchaseOrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_PurchaseReceivingDetails_PurchaseReceivingDetailId",
                        column: x => x.PurchaseReceivingDetailId,
                        principalTable: "PurchaseReceivingDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_PurchaseReturns_PurchaseReturnId",
                        column: x => x.PurchaseReturnId,
                        principalTable: "PurchaseReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProductionScheduleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduledQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CompletedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ProductionItemStatus = table.Column<int>(type: "int", nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    ProductionScheduleId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    ResponsibleEmployeeId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ProductionScheduleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleItems_Employees_ResponsibleEmployeeId",
                        column: x => x.ResponsibleEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionScheduleItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleItems_ProductionSchedules_ProductionScheduleId",
                        column: x => x.ProductionScheduleId,
                        principalTable: "ProductionSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleItems_SalesOrderDetails_SalesOrderDetailId",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionScheduleItems_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionScheduleItems_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesDeliveryDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsSettled = table.Column<bool>(type: "bit", nullable: false),
                    TotalReceivedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalReturnedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    SalesDeliveryId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SalesDeliveryDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_SalesDeliveries_SalesDeliveryId",
                        column: x => x.SalesDeliveryId,
                        principalTable: "SalesDeliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_SalesOrderDetails_SalesOrderDetailId",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesDeliveryDetails_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderCompositionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: false),
                    ComponentItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    ComponentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
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
                    table.PrimaryKey("PK_SalesOrderCompositionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderCompositionDetails_Items_ComponentItemId",
                        column: x => x.ComponentItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderCompositionDetails_SalesOrderDetails_SalesOrderDetailId",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderCompositionDetails_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductionScheduleAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllocatedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ProductionScheduleItemId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ProductionScheduleAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleAllocations_ProductionScheduleItems_ProductionScheduleItemId",
                        column: x => x.ProductionScheduleItemId,
                        principalTable: "ProductionScheduleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleAllocations_SalesOrderDetails_SalesOrderDetailId",
                        column: x => x.SalesOrderDetailId,
                        principalTable: "SalesOrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionScheduleCompletions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompletedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualUnitCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QualityCheckResult = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompletedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    ProductionScheduleItemId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    InventoryTransactionId = table.Column<int>(type: "int", nullable: true),
                    IsJournalized = table.Column<bool>(type: "bit", nullable: false),
                    JournalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ProductionScheduleCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleCompletions_Employees_CompletedByEmployeeId",
                        column: x => x.CompletedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionScheduleCompletions_InventoryTransactions_InventoryTransactionId",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionScheduleCompletions_ProductionScheduleItems_ProductionScheduleItemId",
                        column: x => x.ProductionScheduleItemId,
                        principalTable: "ProductionScheduleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleCompletions_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionScheduleCompletions_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductionScheduleDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionScheduleItemId = table.Column<int>(type: "int", nullable: false),
                    ComponentItemId = table.Column<int>(type: "int", nullable: false),
                    ItemCompositionDetailId = table.Column<int>(type: "int", nullable: true),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IssuedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    EstimatedUnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ActualUnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    ActualUsedQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReturnQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReturnWarehouseId = table.Column<int>(type: "int", nullable: true),
                    ReturnLocationId = table.Column<int>(type: "int", nullable: true),
                    ScrapQty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ScrapReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ProductionScheduleDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleDetails_ItemCompositionDetails_ItemCompositionDetailId",
                        column: x => x.ItemCompositionDetailId,
                        principalTable: "ItemCompositionDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleDetails_Items_ComponentItemId",
                        column: x => x.ComponentItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleDetails_ProductionScheduleItems_ProductionScheduleItemId",
                        column: x => x.ProductionScheduleItemId,
                        principalTable: "ProductionScheduleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionScheduleDetails_WarehouseLocations_ReturnLocationId",
                        column: x => x.ReturnLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionScheduleDetails_Warehouses_ReturnWarehouseId",
                        column: x => x.ReturnWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionScheduleDetails_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OriginalUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsSettled = table.Column<bool>(type: "bit", nullable: false),
                    TotalPaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesReturnId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SalesDeliveryDetailId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_SalesReturnDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_SalesDeliveryDetails_SalesDeliveryDetailId",
                        column: x => x.SalesDeliveryDetailId,
                        principalTable: "SalesDeliveryDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_SalesReturns_SalesReturnId",
                        column: x => x.SalesReturnId,
                        principalTable: "SalesReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialIssueDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialIssueId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    WarehouseLocationId = table.Column<int>(type: "int", nullable: true),
                    IssueQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ProductionScheduleDetailId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_MaterialIssueDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialIssueDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialIssueDetails_MaterialIssues_MaterialIssueId",
                        column: x => x.MaterialIssueId,
                        principalTable: "MaterialIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialIssueDetails_ProductionScheduleDetails_ProductionScheduleDetailId",
                        column: x => x.ProductionScheduleDetailId,
                        principalTable: "ProductionScheduleDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaterialIssueDetails_WarehouseLocations_WarehouseLocationId",
                        column: x => x.WarehouseLocationId,
                        principalTable: "WarehouseLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaterialIssueDetails_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountItems_AccountLevel",
                table: "AccountItems",
                column: "AccountLevel");

            migrationBuilder.CreateIndex(
                name: "IX_AccountItems_AccountType",
                table: "AccountItems",
                column: "AccountType");

            migrationBuilder.CreateIndex(
                name: "IX_AccountItems_Code",
                table: "AccountItems",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountItems_IsDetailAccount",
                table: "AccountItems",
                column: "IsDetailAccount");

            migrationBuilder.CreateIndex(
                name: "IX_AccountItems_LinkedCustomerId",
                table: "AccountItems",
                column: "LinkedCustomerId",
                filter: "[LinkedCustomerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountItems_LinkedItemId",
                table: "AccountItems",
                column: "LinkedItemId",
                filter: "[LinkedItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountItems_LinkedSupplierId",
                table: "AccountItems",
                column: "LinkedSupplierId",
                filter: "[LinkedSupplierId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountItems_ParentId",
                table: "AccountItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessCards_OwnerType_OwnerId",
                table: "BusinessCards",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_EmployeeId",
                table: "CalendarEvents",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Code",
                table: "Companies",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBankAccounts_BankId",
                table: "CompanyBankAccounts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBankAccounts_CompanyId",
                table: "CompanyBankAccounts",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyModules_ModuleKey",
                table: "CompanyModules",
                column: "ModuleKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompositionCategories_Name",
                table: "CompositionCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBankAccounts_BankId",
                table: "CustomerBankAccounts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBankAccounts_CustomerId",
                table: "CustomerBankAccounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerComplaints_CustomerId",
                table: "CustomerComplaints",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerComplaints_EmployeeId",
                table: "CustomerComplaints",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Code",
                table: "Customers",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_EmployeeId",
                table: "Customers",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PaymentMethodId",
                table: "Customers",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVisits_CustomerId",
                table: "CustomerVisits",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVisits_EmployeeId",
                table: "CustomerVisits",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedRecords_DeletedAt",
                table: "DeletedRecords",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedRecords_TableName_RecordId",
                table: "DeletedRecords",
                columns: new[] { "TableName", "RecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DeputyManagerId",
                table: "Departments",
                column: "DeputyManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerId",
                table: "Departments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFiles_DocumentId",
                table: "DocumentFiles",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentCategoryId",
                table: "Documents",
                column: "DocumentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBankAccounts_BankId",
                table: "EmployeeBankAccounts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBankAccounts_EmployeeId",
                table: "EmployeeBankAccounts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardConfigs_EmployeeId",
                table: "EmployeeDashboardConfigs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardConfigs_PanelId_NavigationItemKey",
                table: "EmployeeDashboardConfigs",
                columns: new[] { "PanelId", "NavigationItemKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardConfigs_PanelId_SortOrder",
                table: "EmployeeDashboardConfigs",
                columns: new[] { "PanelId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDashboardPanels_EmployeeId_SortOrder",
                table: "EmployeeDashboardPanels",
                columns: new[] { "EmployeeId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLicenses_EmployeeId",
                table: "EmployeeLicenses",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePreferences_EmployeeId",
                table: "EmployeePreferences",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Account",
                table: "Employees",
                column: "Account",
                unique: true,
                filter: "[Account] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Code",
                table: "Employees",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeePositionId",
                table: "Employees",
                column: "EmployeePositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_RoleId",
                table: "Employees",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaries_EffectiveDate",
                table: "EmployeeSalaries",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaries_EmployeeId",
                table: "EmployeeSalaries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryItems_EmployeeSalaryId",
                table: "EmployeeSalaryItems",
                column: "EmployeeSalaryId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryItems_PayrollItemId",
                table: "EmployeeSalaryItems",
                column: "PayrollItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTools_EmployeeId",
                table: "EmployeeTools",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTrainingRecords_EmployeeId",
                table: "EmployeeTrainingRecords",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalPeriods_CompanyId",
                table: "FiscalPeriods",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalPeriods_FiscalYear",
                table: "FiscalPeriods",
                column: "FiscalYear");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalPeriods_FiscalYear_PeriodNumber_CompanyId",
                table: "FiscalPeriods",
                columns: new[] { "FiscalYear", "PeriodNumber", "CompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_InventoryStockDetailId",
                table: "InventoryReservations",
                column: "InventoryStockDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_InventoryStockId",
                table: "InventoryReservations",
                column: "InventoryStockId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_ItemId_ReservationDate",
                table: "InventoryReservations",
                columns: new[] { "ItemId", "ReservationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_ReferenceNumber",
                table: "InventoryReservations",
                column: "ReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_ReservationType_ReservationStatus",
                table: "InventoryReservations",
                columns: new[] { "ReservationType", "ReservationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_WarehouseId",
                table: "InventoryReservations",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_WarehouseLocationId",
                table: "InventoryReservations",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStockDetails_InventoryStockId_WarehouseId_WarehouseLocationId_BatchNumber",
                table: "InventoryStockDetails",
                columns: new[] { "InventoryStockId", "WarehouseId", "WarehouseLocationId", "BatchNumber" },
                unique: true,
                filter: "[WarehouseLocationId] IS NOT NULL AND [BatchNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStockDetails_WarehouseId",
                table: "InventoryStockDetails",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStockDetails_WarehouseLocationId",
                table: "InventoryStockDetails",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ItemId",
                table: "InventoryStocks",
                column: "ItemId",
                unique: true,
                filter: "[ItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionDetails_InventoryStockDetailId",
                table: "InventoryTransactionDetails",
                column: "InventoryStockDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionDetails_InventoryStockId",
                table: "InventoryTransactionDetails",
                column: "InventoryStockId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionDetails_InventoryTransactionId",
                table: "InventoryTransactionDetails",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionDetails_ItemId",
                table: "InventoryTransactionDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionDetails_WarehouseLocationId",
                table: "InventoryTransactionDetails",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_EmployeeId",
                table: "InventoryTransactions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_InventoryStockId",
                table: "InventoryTransactions",
                column: "InventoryStockId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_SourceDocumentType_SourceDocumentId",
                table: "InventoryTransactions",
                columns: new[] { "SourceDocumentType", "SourceDocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TransactionNumber",
                table: "InventoryTransactions",
                column: "TransactionNumber");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TransactionType_TransactionDate",
                table: "InventoryTransactions",
                columns: new[] { "TransactionType", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_WarehouseId_TransactionDate",
                table: "InventoryTransactions",
                columns: new[] { "WarehouseId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionTypes_Code",
                table: "InventoryTransactionTypes",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategories_Code",
                table: "ItemCategories",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCompositionDetails_ComponentItemId",
                table: "ItemCompositionDetails",
                column: "ComponentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCompositionDetails_ItemCompositionId_ComponentItemId",
                table: "ItemCompositionDetails",
                columns: new[] { "ItemCompositionId", "ComponentItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCompositionDetails_UnitId",
                table: "ItemCompositionDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCompositions_CompositionCategoryId",
                table: "ItemCompositions",
                column: "CompositionCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCompositions_CreatedByEmployeeId",
                table: "ItemCompositions",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCompositions_CustomerId",
                table: "ItemCompositions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCompositions_ParentItemId",
                table: "ItemCompositions",
                column: "ParentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCustomer_CustomerId",
                table: "ItemCustomers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCustomer_ItemId_IsPreferred_Priority",
                table: "ItemCustomers",
                columns: new[] { "ItemId", "IsPreferred", "Priority" });

            migrationBuilder.CreateIndex(
                name: "UX_ItemCustomer_ItemId_CustomerId",
                table: "ItemCustomers",
                columns: new[] { "ItemId", "CustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemPhotos_ItemId",
                table: "ItemPhotos",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Code",
                table: "Items",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemCategoryId",
                table: "Items",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ProductionUnitId",
                table: "Items",
                column: "ProductionUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SizeId",
                table: "Items",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_UnitId",
                table: "Items",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSupplier_ItemId_IsPreferred_Priority",
                table: "ItemSuppliers",
                columns: new[] { "ItemId", "IsPreferred", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemSupplier_SupplierId",
                table: "ItemSuppliers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "UX_ItemSupplier_ItemId_SupplierId",
                table: "ItemSuppliers",
                columns: new[] { "ItemId", "SupplierId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_Code",
                table: "JournalEntries",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CompanyId",
                table: "JournalEntries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryDate",
                table: "JournalEntries",
                column: "EntryDate");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryType",
                table: "JournalEntries",
                column: "EntryType");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_FiscalYear_FiscalPeriod",
                table: "JournalEntries",
                columns: new[] { "FiscalYear", "FiscalPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_JournalEntryStatus",
                table: "JournalEntries",
                column: "JournalEntryStatus");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReversalEntryId",
                table: "JournalEntries",
                column: "ReversalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReversedEntryId",
                table: "JournalEntries",
                column: "ReversedEntryId");

            migrationBuilder.CreateIndex(
                name: "UX_JournalEntry_SourceDocument",
                table: "JournalEntries",
                columns: new[] { "SourceDocumentType", "SourceDocumentId" },
                unique: true,
                filter: "[SourceDocumentType] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_AccountItemId",
                table: "JournalEntryLines",
                column: "AccountItemId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_AccountItemId_Direction",
                table: "JournalEntryLines",
                columns: new[] { "AccountItemId", "Direction" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryId",
                table: "JournalEntryLines",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssueDetails_ItemId",
                table: "MaterialIssueDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssueDetails_MaterialIssueId_ItemId",
                table: "MaterialIssueDetails",
                columns: new[] { "MaterialIssueId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssueDetails_ProductionScheduleDetailId",
                table: "MaterialIssueDetails",
                column: "ProductionScheduleDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssueDetails_WarehouseId",
                table: "MaterialIssueDetails",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssueDetails_WarehouseLocationId",
                table: "MaterialIssueDetails",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_Code",
                table: "MaterialIssues",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_ConfirmedByEmployeeId",
                table: "MaterialIssues",
                column: "ConfirmedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_DepartmentId_IssueDate",
                table: "MaterialIssues",
                columns: new[] { "DepartmentId", "IssueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_EmployeeId_IssueDate",
                table: "MaterialIssues",
                columns: new[] { "EmployeeId", "IssueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_IsConfirmed",
                table: "MaterialIssues",
                column: "IsConfirmed");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_IsJournalized",
                table: "MaterialIssues",
                column: "IsJournalized");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_IssueDate",
                table: "MaterialIssues",
                column: "IssueDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialIssues_ProductionScheduleId",
                table: "MaterialIssues",
                column: "ProductionScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyAttendanceSummaries_Year_Month",
                table: "MonthlyAttendanceSummaries",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "UX_MonthlyAttendance_Employee_Period",
                table: "MonthlyAttendanceSummaries",
                columns: new[] { "EmployeeId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaperSettings_Name",
                table: "PaperSettings",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollItems_Code",
                table: "PayrollItems",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_Year_Month",
                table: "PayrollPeriods",
                columns: new[] { "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecordDetails_PayrollItemId",
                table: "PayrollRecordDetails",
                column: "PayrollItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecordDetails_PayrollRecordId",
                table: "PayrollRecordDetails",
                column: "PayrollRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_EmployeeId",
                table: "PayrollRecords",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_PayrollPeriodId_EmployeeId",
                table: "PayrollRecords",
                columns: new[] { "PayrollPeriodId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_ChangeDate",
                table: "PriceHistories",
                column: "ChangeDate");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_ItemId_PriceType_ChangeDate",
                table: "PriceHistories",
                columns: new[] { "ItemId", "PriceType", "ChangeDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_RelatedCustomerId",
                table: "PriceHistories",
                column: "RelatedCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_RelatedSupplierId",
                table: "PriceHistories",
                column: "RelatedSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleAllocations_ProductionScheduleItemId_SalesOrderDetailId",
                table: "ProductionScheduleAllocations",
                columns: new[] { "ProductionScheduleItemId", "SalesOrderDetailId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleAllocations_SalesOrderDetailId",
                table: "ProductionScheduleAllocations",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleCompletions_CompletedByEmployeeId",
                table: "ProductionScheduleCompletions",
                column: "CompletedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleCompletions_CompletionDate",
                table: "ProductionScheduleCompletions",
                column: "CompletionDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleCompletions_InventoryTransactionId",
                table: "ProductionScheduleCompletions",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleCompletions_ProductionScheduleItemId",
                table: "ProductionScheduleCompletions",
                column: "ProductionScheduleItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleCompletions_WarehouseId",
                table: "ProductionScheduleCompletions",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleCompletions_WarehouseLocationId",
                table: "ProductionScheduleCompletions",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleDetails_ComponentItemId",
                table: "ProductionScheduleDetails",
                column: "ComponentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleDetails_ItemCompositionDetailId",
                table: "ProductionScheduleDetails",
                column: "ItemCompositionDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleDetails_ProductionScheduleItemId_ComponentItemId",
                table: "ProductionScheduleDetails",
                columns: new[] { "ProductionScheduleItemId", "ComponentItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleDetails_ReturnLocationId",
                table: "ProductionScheduleDetails",
                column: "ReturnLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleDetails_ReturnWarehouseId",
                table: "ProductionScheduleDetails",
                column: "ReturnWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleDetails_WarehouseId",
                table: "ProductionScheduleDetails",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleItems_Code",
                table: "ProductionScheduleItems",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleItems_ItemId",
                table: "ProductionScheduleItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleItems_ProductionItemStatus",
                table: "ProductionScheduleItems",
                column: "ProductionItemStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleItems_ProductionScheduleId_ItemId",
                table: "ProductionScheduleItems",
                columns: new[] { "ProductionScheduleId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleItems_ResponsibleEmployeeId",
                table: "ProductionScheduleItems",
                column: "ResponsibleEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleItems_SalesOrderDetailId",
                table: "ProductionScheduleItems",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleItems_WarehouseId",
                table: "ProductionScheduleItems",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionScheduleItems_WarehouseLocationId",
                table: "ProductionScheduleItems",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_Code",
                table: "ProductionSchedules",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_CreatedByEmployeeId",
                table: "ProductionSchedules",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_CustomerId",
                table: "ProductionSchedules",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_SalesOrderId",
                table: "ProductionSchedules",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionSchedules_ScheduleDate",
                table: "ProductionSchedules",
                column: "ScheduleDate");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_CompletedByEmployeeId",
                table: "PurchaseOrderDetails",
                column: "CompletedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_ItemId",
                table: "PurchaseOrderDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_PurchaseOrderId_ItemId",
                table: "PurchaseOrderDetails",
                columns: new[] { "PurchaseOrderId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_ApprovedBy",
                table: "PurchaseOrders",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Code",
                table: "PurchaseOrders",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CompanyId",
                table: "PurchaseOrders",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierId_OrderDate",
                table: "PurchaseOrders",
                columns: new[] { "SupplierId", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_WarehouseId",
                table: "PurchaseOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivingDetails_ItemId",
                table: "PurchaseReceivingDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivingDetails_PurchaseOrderDetailId",
                table: "PurchaseReceivingDetails",
                column: "PurchaseOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivingDetails_PurchaseReceivingId_ItemId",
                table: "PurchaseReceivingDetails",
                columns: new[] { "PurchaseReceivingId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivingDetails_WarehouseId",
                table: "PurchaseReceivingDetails",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivingDetails_WarehouseLocationId",
                table: "PurchaseReceivingDetails",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_ApprovedBy",
                table: "PurchaseReceivings",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_Code",
                table: "PurchaseReceivings",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceivings_SupplierId_ReceiptDate",
                table: "PurchaseReceivings",
                columns: new[] { "SupplierId", "ReceiptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_ItemId",
                table: "PurchaseReturnDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_PurchaseOrderDetailId",
                table: "PurchaseReturnDetails",
                column: "PurchaseOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_PurchaseReceivingDetailId",
                table: "PurchaseReturnDetails",
                column: "PurchaseReceivingDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_PurchaseReturnId_ItemId",
                table: "PurchaseReturnDetails",
                columns: new[] { "PurchaseReturnId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_UnitId",
                table: "PurchaseReturnDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_WarehouseLocationId",
                table: "PurchaseReturnDetails",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_ApprovedBy",
                table: "PurchaseReturns",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_Code",
                table: "PurchaseReturns",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_ReturnReasonId",
                table: "PurchaseReturns",
                column: "ReturnReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_SupplierId_ReturnDate",
                table: "PurchaseReturns",
                columns: new[] { "SupplierId", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationCompositionDetails_ComponentItemId",
                table: "QuotationCompositionDetails",
                column: "ComponentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationCompositionDetails_QuotationDetailId_ComponentItemId",
                table: "QuotationCompositionDetails",
                columns: new[] { "QuotationDetailId", "ComponentItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationCompositionDetails_UnitId",
                table: "QuotationCompositionDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationDetails_ItemId",
                table: "QuotationDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationDetails_QuotationId_ItemId",
                table: "QuotationDetails",
                columns: new[] { "QuotationId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationDetails_UnitId",
                table: "QuotationDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationPhotos_QuotationId",
                table: "QuotationPhotos",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_ApprovedBy",
                table: "Quotations",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_Code",
                table: "Quotations",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_CompanyId",
                table: "Quotations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_CustomerId_QuotationDate",
                table: "Quotations",
                columns: new[] { "CustomerId", "QuotationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_EmployeeId",
                table: "Quotations",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_SalespersonId",
                table: "Quotations",
                column: "SalespersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportPrintConfigurations_PaperSettingId",
                table: "ReportPrintConfigurations",
                column: "PaperSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportPrintConfigurations_ReportName",
                table: "ReportPrintConfigurations",
                column: "ReportName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_ApprovedBy",
                table: "SalesDeliveries",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_Code",
                table: "SalesDeliveries",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_CustomerId_DeliveryDate",
                table: "SalesDeliveries",
                columns: new[] { "CustomerId", "DeliveryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_EmployeeId",
                table: "SalesDeliveries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_SalespersonId",
                table: "SalesDeliveries",
                column: "SalespersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveries_WarehouseId",
                table: "SalesDeliveries",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_ItemId",
                table: "SalesDeliveryDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_SalesDeliveryId_ItemId",
                table: "SalesDeliveryDetails",
                columns: new[] { "SalesDeliveryId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_SalesOrderDetailId",
                table: "SalesDeliveryDetails",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_UnitId",
                table: "SalesDeliveryDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_WarehouseId",
                table: "SalesDeliveryDetails",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryDetails_WarehouseLocationId",
                table: "SalesDeliveryDetails",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderCompositionDetails_ComponentItemId",
                table: "SalesOrderCompositionDetails",
                column: "ComponentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderCompositionDetails_SalesOrderDetailId_ComponentItemId",
                table: "SalesOrderCompositionDetails",
                columns: new[] { "SalesOrderDetailId", "ComponentItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderCompositionDetails_UnitId",
                table: "SalesOrderCompositionDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_ItemId",
                table: "SalesOrderDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_QuotationDetailId",
                table: "SalesOrderDetails",
                column: "QuotationDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_SalesOrderId_ItemId",
                table: "SalesOrderDetails",
                columns: new[] { "SalesOrderId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_UnitId",
                table: "SalesOrderDetails",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_WarehouseId",
                table: "SalesOrderDetails",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_WarehouseLocationId",
                table: "SalesOrderDetails",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderPhotos_SalesOrderId",
                table: "SalesOrderPhotos",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_ApprovedBy",
                table: "SalesOrders",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_Code",
                table: "SalesOrders",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CompanyId_OrderDate",
                table: "SalesOrders",
                columns: new[] { "CompanyId", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CustomerId_OrderDate",
                table: "SalesOrders",
                columns: new[] { "CustomerId", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_EmployeeId",
                table: "SalesOrders",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_SalespersonId",
                table: "SalesOrders",
                column: "SalespersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_ItemId",
                table: "SalesReturnDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_SalesDeliveryDetailId",
                table: "SalesReturnDetails",
                column: "SalesDeliveryDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_SalesReturnId_ItemId",
                table: "SalesReturnDetails",
                columns: new[] { "SalesReturnId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnReasons_Name",
                table: "SalesReturnReasons",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_ApprovedBy",
                table: "SalesReturns",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_Code",
                table: "SalesReturns",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_CustomerId_ReturnDate",
                table: "SalesReturns",
                columns: new[] { "CustomerId", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_EmployeeId",
                table: "SalesReturns",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_ReturnReasonId",
                table: "SalesReturns",
                column: "ReturnReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargets_SalespersonId",
                table: "SalesTargets",
                column: "SalespersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTargets_Year_Month_SalespersonId",
                table: "SalesTargets",
                columns: new[] { "Year", "Month", "SalespersonId" },
                unique: true,
                filter: "[Month] IS NOT NULL AND [SalespersonId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleRecords_Code",
                table: "ScaleRecords",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleRecords_CustomerId_RecordDate",
                table: "ScaleRecords",
                columns: new[] { "CustomerId", "RecordDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ScaleRecords_ItemId",
                table: "ScaleRecords",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleRecords_VehicleId_RecordDate",
                table: "ScaleRecords",
                columns: new[] { "VehicleId", "RecordDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ScaleRecords_WarehouseId",
                table: "ScaleRecords",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleRecords_WarehouseLocationId",
                table: "ScaleRecords",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleTypes_Code",
                table: "ScaleTypes",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleTypes_ItemId",
                table: "ScaleTypes",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffDocuments_Code",
                table: "SetoffDocuments",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffDocuments_CompanyId",
                table: "SetoffDocuments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffDocuments_RelatedPartyId",
                table: "SetoffDocuments",
                column: "RelatedPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffDocuments_SetoffDate",
                table: "SetoffDocuments",
                column: "SetoffDate");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffDocuments_SetoffType",
                table: "SetoffDocuments",
                column: "SetoffType");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffItemDetails_ItemId",
                table: "SetoffItemDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffItemDetails_SetoffDocumentId",
                table: "SetoffItemDetails",
                column: "SetoffDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffItemDetails_SourceDetailType_SourceDetailId",
                table: "SetoffItemDetails",
                columns: new[] { "SourceDetailType", "SourceDetailId" });

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPayments_BankId",
                table: "SetoffPayments",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPayments_PaymentMethodId",
                table: "SetoffPayments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPayments_SetoffDocumentId",
                table: "SetoffPayments",
                column: "SetoffDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_CustomerId",
                table: "SetoffPrepayments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_PrepaymentTypeId",
                table: "SetoffPrepayments",
                column: "PrepaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_SetoffDocumentId",
                table: "SetoffPrepayments",
                column: "SetoffDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_SourceDocumentCode",
                table: "SetoffPrepayments",
                column: "SourceDocumentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_SourcePrepaymentId",
                table: "SetoffPrepayments",
                column: "SourcePrepaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepayments_SupplierId",
                table: "SetoffPrepayments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepaymentUsages_SetoffDocumentId",
                table: "SetoffPrepaymentUsages",
                column: "SetoffDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepaymentUsages_SetoffPrepaymentId",
                table: "SetoffPrepaymentUsages",
                column: "SetoffPrepaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_SetoffPrepaymentUsages_UsageDate",
                table: "SetoffPrepaymentUsages",
                column: "UsageDate");

            migrationBuilder.CreateIndex(
                name: "IX_Sizes_Code",
                table: "Sizes",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StickyNotes_EmployeeId",
                table: "StickyNotes",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTakingDetails_ItemId",
                table: "StockTakingDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTakingDetails_StockTakingId_ItemId_WarehouseLocationId",
                table: "StockTakingDetails",
                columns: new[] { "StockTakingId", "ItemId", "WarehouseLocationId" },
                unique: true,
                filter: "[WarehouseLocationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StockTakingDetails_WarehouseLocationId",
                table: "StockTakingDetails",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTakings_ApprovedBy",
                table: "StockTakings",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StockTakings_TakingNumber",
                table: "StockTakings",
                column: "TakingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTakings_TakingStatus_TakingDate",
                table: "StockTakings",
                columns: new[] { "TakingStatus", "TakingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTakings_WarehouseId_TakingDate",
                table: "StockTakings",
                columns: new[] { "WarehouseId", "TakingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTakings_WarehouseLocationId",
                table: "StockTakings",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierBankAccounts_BankId",
                table: "SupplierBankAccounts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierBankAccounts_SupplierId",
                table: "SupplierBankAccounts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPricings_ItemId_SupplierId_EffectiveDate",
                table: "SupplierPricings",
                columns: new[] { "ItemId", "SupplierId", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPricings_SupplierId_EffectiveDate",
                table: "SupplierPricings",
                columns: new[] { "SupplierId", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Code",
                table: "Suppliers",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_PaymentMethodId",
                table: "Suppliers",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierVisits_EmployeeId",
                table: "SupplierVisits",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierVisits_SupplierId",
                table: "SupplierVisits",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitConversions_FromUnitId_ToUnitId",
                table: "UnitConversions",
                columns: new[] { "FromUnitId", "ToUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitConversions_ToUnitId",
                table: "UnitConversions",
                column: "ToUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Code",
                table: "Units",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMaintenances_Code",
                table: "VehicleMaintenances",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMaintenances_EmployeeId",
                table: "VehicleMaintenances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMaintenances_VehicleId",
                table: "VehicleMaintenances",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Code",
                table: "Vehicles",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CompanyId",
                table: "Vehicles",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CustomerId",
                table: "Vehicles",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_EmployeeId",
                table: "Vehicles",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_SupplierId",
                table: "Vehicles",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_VehicleTypeId",
                table: "Vehicles",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTypes_Code",
                table: "VehicleTypes",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseLocations_WarehouseId_Code",
                table: "WarehouseLocations",
                columns: new[] { "WarehouseId", "Code" },
                unique: true,
                filter: "[WarehouseId] IS NOT NULL AND [Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                table: "Warehouses",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountItems_Customers_LinkedCustomerId",
                table: "AccountItems",
                column: "LinkedCustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Employees_EmployeeId",
                table: "CalendarEvents",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerBankAccounts_Customers_CustomerId",
                table: "CustomerBankAccounts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerComplaints_Customers_CustomerId",
                table: "CustomerComplaints",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerComplaints_Employees_EmployeeId",
                table: "CustomerComplaints",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Employees_EmployeeId",
                table: "Customers",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerVisits_Employees_EmployeeId",
                table: "CustomerVisits",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_DeputyManagerId",
                table: "Departments",
                column: "DeputyManagerId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_ManagerId",
                table: "Departments",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_DeputyManagerId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_ManagerId",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "BusinessCards");

            migrationBuilder.DropTable(
                name: "CalendarEvents");

            migrationBuilder.DropTable(
                name: "Colors");

            migrationBuilder.DropTable(
                name: "CompanyBankAccounts");

            migrationBuilder.DropTable(
                name: "CompanyModules");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "CustomerBankAccounts");

            migrationBuilder.DropTable(
                name: "CustomerComplaints");

            migrationBuilder.DropTable(
                name: "CustomerVisits");

            migrationBuilder.DropTable(
                name: "DeletedRecords");

            migrationBuilder.DropTable(
                name: "DocumentFiles");

            migrationBuilder.DropTable(
                name: "EmployeeBankAccounts");

            migrationBuilder.DropTable(
                name: "EmployeeDashboardConfigs");

            migrationBuilder.DropTable(
                name: "EmployeeLicenses");

            migrationBuilder.DropTable(
                name: "EmployeePreferences");

            migrationBuilder.DropTable(
                name: "EmployeeSalaryItems");

            migrationBuilder.DropTable(
                name: "EmployeeTools");

            migrationBuilder.DropTable(
                name: "EmployeeTrainingRecords");

            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.DropTable(
                name: "FiscalPeriods");

            migrationBuilder.DropTable(
                name: "HealthInsuranceGrades");

            migrationBuilder.DropTable(
                name: "InsuranceRates");

            migrationBuilder.DropTable(
                name: "InventoryReservations");

            migrationBuilder.DropTable(
                name: "InventoryTransactionDetails");

            migrationBuilder.DropTable(
                name: "InventoryTransactionTypes");

            migrationBuilder.DropTable(
                name: "ItemCustomers");

            migrationBuilder.DropTable(
                name: "ItemPhotos");

            migrationBuilder.DropTable(
                name: "ItemSuppliers");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "LaborInsuranceGrades");

            migrationBuilder.DropTable(
                name: "MaterialIssueDetails");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "MinimumWages");

            migrationBuilder.DropTable(
                name: "MonthlyAttendanceSummaries");

            migrationBuilder.DropTable(
                name: "PayrollRecordDetails");

            migrationBuilder.DropTable(
                name: "PriceHistories");

            migrationBuilder.DropTable(
                name: "ProductionScheduleAllocations");

            migrationBuilder.DropTable(
                name: "ProductionScheduleCompletions");

            migrationBuilder.DropTable(
                name: "PurchaseReturnDetails");

            migrationBuilder.DropTable(
                name: "QuotationCompositionDetails");

            migrationBuilder.DropTable(
                name: "QuotationPhotos");

            migrationBuilder.DropTable(
                name: "ReportPrintConfigurations");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SalesOrderCompositionDetails");

            migrationBuilder.DropTable(
                name: "SalesOrderPhotos");

            migrationBuilder.DropTable(
                name: "SalesReturnDetails");

            migrationBuilder.DropTable(
                name: "SalesTargets");

            migrationBuilder.DropTable(
                name: "ScaleRecords");

            migrationBuilder.DropTable(
                name: "ScaleTypes");

            migrationBuilder.DropTable(
                name: "SetoffItemDetails");

            migrationBuilder.DropTable(
                name: "SetoffPayments");

            migrationBuilder.DropTable(
                name: "SetoffPrepaymentUsages");

            migrationBuilder.DropTable(
                name: "StickyNotes");

            migrationBuilder.DropTable(
                name: "StockTakingDetails");

            migrationBuilder.DropTable(
                name: "SupplierBankAccounts");

            migrationBuilder.DropTable(
                name: "SupplierPricings");

            migrationBuilder.DropTable(
                name: "SupplierVisits");

            migrationBuilder.DropTable(
                name: "SystemParameters");

            migrationBuilder.DropTable(
                name: "TextMessageTemplates");

            migrationBuilder.DropTable(
                name: "UnitConversions");

            migrationBuilder.DropTable(
                name: "VehicleMaintenances");

            migrationBuilder.DropTable(
                name: "Weathers");

            migrationBuilder.DropTable(
                name: "WithholdingTaxTables");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "EmployeeDashboardPanels");

            migrationBuilder.DropTable(
                name: "EmployeeSalaries");

            migrationBuilder.DropTable(
                name: "InventoryStockDetails");

            migrationBuilder.DropTable(
                name: "AccountItems");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "MaterialIssues");

            migrationBuilder.DropTable(
                name: "ProductionScheduleDetails");

            migrationBuilder.DropTable(
                name: "PayrollItems");

            migrationBuilder.DropTable(
                name: "PayrollRecords");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "PurchaseReceivingDetails");

            migrationBuilder.DropTable(
                name: "PurchaseReturns");

            migrationBuilder.DropTable(
                name: "PaperSettings");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "SalesDeliveryDetails");

            migrationBuilder.DropTable(
                name: "SalesReturns");

            migrationBuilder.DropTable(
                name: "SetoffPrepayments");

            migrationBuilder.DropTable(
                name: "StockTakings");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "DocumentCategories");

            migrationBuilder.DropTable(
                name: "ItemCompositionDetails");

            migrationBuilder.DropTable(
                name: "ProductionScheduleItems");

            migrationBuilder.DropTable(
                name: "PayrollPeriods");

            migrationBuilder.DropTable(
                name: "InventoryStocks");

            migrationBuilder.DropTable(
                name: "PurchaseOrderDetails");

            migrationBuilder.DropTable(
                name: "PurchaseReceivings");

            migrationBuilder.DropTable(
                name: "PurchaseReturnReasons");

            migrationBuilder.DropTable(
                name: "SalesDeliveries");

            migrationBuilder.DropTable(
                name: "SalesReturnReasons");

            migrationBuilder.DropTable(
                name: "PrepaymentTypes");

            migrationBuilder.DropTable(
                name: "SetoffDocuments");

            migrationBuilder.DropTable(
                name: "VehicleTypes");

            migrationBuilder.DropTable(
                name: "ItemCompositions");

            migrationBuilder.DropTable(
                name: "ProductionSchedules");

            migrationBuilder.DropTable(
                name: "SalesOrderDetails");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "CompositionCategories");

            migrationBuilder.DropTable(
                name: "QuotationDetails");

            migrationBuilder.DropTable(
                name: "SalesOrders");

            migrationBuilder.DropTable(
                name: "WarehouseLocations");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Quotations");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "ItemCategories");

            migrationBuilder.DropTable(
                name: "Sizes");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "EmployeePositions");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
