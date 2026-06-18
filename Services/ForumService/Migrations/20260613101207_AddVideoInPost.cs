using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForumService.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoInPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "Videos",
                table: "Threads",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Videos",
                table: "Threads");
        }
    }
}
