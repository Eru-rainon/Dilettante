using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dilettante.Migrations
{
    /// <inheritdoc />
    public partial class AddDevsAndPubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Developers",
                table: "Games",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publishers",
                table: "Games",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Developers",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Publishers",
                table: "Games");
        }
    }
}
