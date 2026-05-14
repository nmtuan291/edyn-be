using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForumService.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorDetailsToThread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorAvatar",
                table: "Threads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatorName",
                table: "Threads",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorAvatar",
                table: "Threads");

            migrationBuilder.DropColumn(
                name: "CreatorName",
                table: "Threads");
        }
    }
}
