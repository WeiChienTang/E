using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RenameIndustryToIndustryTypeSafe : Migration
    {        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create new IndustryTypes table
            migrationBuilder.CreateTable(
                name: "IndustryTypes",
                columns: table => new
                {
                    IndustryTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndustryTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IndustryTypeCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryTypes", x => x.IndustryTypeId);
                });

            // Step 2: Copy data from Industries to IndustryTypes
            migrationBuilder.Sql(@"
                INSERT INTO [IndustryTypes] ([IndustryTypeName], [IndustryTypeCode], [Status])
                SELECT [IndustryName], [IndustryCode], [Status]
                FROM [Industries]
            ");

            // Step 3: Add new column to Customers table
            migrationBuilder.AddColumn<int>(
                name: "IndustryTypeId",
                table: "Customers",
                type: "int",
                nullable: true);

            // Step 4: Update new column with corresponding data
            migrationBuilder.Sql(@"
                UPDATE [Customers] 
                SET [IndustryTypeId] = it.[IndustryTypeId]
                FROM [Customers] c
                INNER JOIN [Industries] i ON c.[IndustryId] = i.[IndustryId]
                INNER JOIN [IndustryTypes] it ON i.[IndustryName] = it.[IndustryTypeName]
            ");

            // Step 5: Drop old foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Industries_IndustryId",
                table: "Customers");

            // Step 6: Drop old index
            migrationBuilder.DropIndex(
                name: "IX_Customers_IndustryId",
                table: "Customers");

            // Step 7: Drop old column
            migrationBuilder.DropColumn(
                name: "IndustryId",
                table: "Customers");

            // Step 8: Drop old table
            migrationBuilder.DropTable(
                name: "Industries");

            // Step 9: Create new index
            migrationBuilder.CreateIndex(
                name: "IX_Customers_IndustryTypeId",
                table: "Customers",
                column: "IndustryTypeId");

            // Step 10: Create new foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Customers_IndustryTypes_IndustryTypeId",
                table: "Customers",
                column: "IndustryTypeId",
                principalTable: "IndustryTypes",
                principalColumn: "IndustryTypeId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_IndustryTypes_IndustryTypeId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "IndustryTypes");

            migrationBuilder.RenameColumn(
                name: "IndustryTypeId",
                table: "Customers",
                newName: "IndustryId");

            migrationBuilder.RenameIndex(
                name: "IX_Customers_IndustryTypeId",
                table: "Customers",
                newName: "IX_Customers_IndustryId");

            migrationBuilder.CreateTable(
                name: "Industries",
                columns: table => new
                {
                    IndustryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndustryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IndustryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Industries", x => x.IndustryId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Industries_IndustryId",
                table: "Customers",
                column: "IndustryId",
                principalTable: "Industries",
                principalColumn: "IndustryId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
