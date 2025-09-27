using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToAccountsReceivableSetoffs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add the CompanyId column with nullable temporary state
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "AccountsReceivableSetoffs",
                type: "int",
                nullable: true,
                defaultValue: null);

            // Step 2: Update existing records with the first available Company ID
            migrationBuilder.Sql(@"
                UPDATE AccountsReceivableSetoffs 
                SET CompanyId = (SELECT TOP 1 Id FROM Companies ORDER BY Id)
                WHERE CompanyId IS NULL");

            // Step 3: Make the column NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "AccountsReceivableSetoffs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Step 4: Create the index
            migrationBuilder.CreateIndex(
                name: "IX_AccountsReceivableSetoffs_CompanyId",
                table: "AccountsReceivableSetoffs",
                column: "CompanyId");

            // Step 5: Add the foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_AccountsReceivableSetoffs_Companies_CompanyId",
                table: "AccountsReceivableSetoffs",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountsReceivableSetoffs_Companies_CompanyId",
                table: "AccountsReceivableSetoffs");

            migrationBuilder.DropIndex(
                name: "IX_AccountsReceivableSetoffs_CompanyId",
                table: "AccountsReceivableSetoffs");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "AccountsReceivableSetoffs");
        }
    }
}
