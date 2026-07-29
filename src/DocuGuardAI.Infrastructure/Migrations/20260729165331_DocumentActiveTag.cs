using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocuGuardAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DocumentActiveTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Documents",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Documents");
        }
    }
}
