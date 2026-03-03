using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddHideApprovalInfoSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HideApprovalInfoSection",
                table: "SystemParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HideApprovalInfoSection",
                table: "SystemParameters");
        }
    }
}
