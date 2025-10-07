using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class 新增PrepaymentType資料表 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrepaymentType",
                table: "SetoffPrepayments",
                newName: "PrepaymentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_SetoffPrepayments_PrepaymentType",
                table: "SetoffPrepayments",
                newName: "IX_SetoffPrepayments_PrepaymentTypeId");

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
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrepaymentTypes", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_SetoffPrepayments_PrepaymentTypes_PrepaymentTypeId",
                table: "SetoffPrepayments",
                column: "PrepaymentTypeId",
                principalTable: "PrepaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetoffPrepayments_PrepaymentTypes_PrepaymentTypeId",
                table: "SetoffPrepayments");

            migrationBuilder.DropTable(
                name: "PrepaymentTypes");

            migrationBuilder.RenameColumn(
                name: "PrepaymentTypeId",
                table: "SetoffPrepayments",
                newName: "PrepaymentType");

            migrationBuilder.RenameIndex(
                name: "IX_SetoffPrepayments_PrepaymentTypeId",
                table: "SetoffPrepayments",
                newName: "IX_SetoffPrepayments_PrepaymentType");
        }
    }
}
