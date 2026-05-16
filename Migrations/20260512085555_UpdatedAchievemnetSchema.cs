using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dilettante.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedAchievemnetSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Achievements",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Achievements");
        }
    }
}
