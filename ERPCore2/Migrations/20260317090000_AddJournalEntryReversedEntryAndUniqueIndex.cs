using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalEntryReversedEntryAndUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bug-3：新增 ReversedEntryId 欄位（沖銷傳票 B → 原傳票 A 的反向 FK）
            migrationBuilder.AddColumn<int>(
                name: "ReversedEntryId",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReversedEntryId",
                table: "JournalEntries",
                column: "ReversedEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_JournalEntries_ReversedEntryId",
                table: "JournalEntries",
                column: "ReversedEntryId",
                principalTable: "JournalEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Bug-9：來源單據唯一索引已存在於 DB（由 AppDbContext 設定早於此 Migration），無需重建
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_JournalEntries_ReversedEntryId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_ReversedEntryId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReversedEntryId",
                table: "JournalEntries");
        }
    }
}
