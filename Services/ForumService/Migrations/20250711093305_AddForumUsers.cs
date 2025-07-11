using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForumService.Migrations
{
    /// <inheritdoc />
    public partial class AddForumUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumUser_Forums_ForumId",
                table: "ForumUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ForumUser",
                table: "ForumUser");

            migrationBuilder.RenameTable(
                name: "ForumUser",
                newName: "ForumUsers");

            migrationBuilder.RenameIndex(
                name: "IX_ForumUser_ForumId",
                table: "ForumUsers",
                newName: "IX_ForumUsers_ForumId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForumUsers",
                table: "ForumUsers",
                columns: new[] { "UserId", "ForumId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ForumUsers_Forums_ForumId",
                table: "ForumUsers",
                column: "ForumId",
                principalTable: "Forums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumUsers_Forums_ForumId",
                table: "ForumUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ForumUsers",
                table: "ForumUsers");

            migrationBuilder.RenameTable(
                name: "ForumUsers",
                newName: "ForumUser");

            migrationBuilder.RenameIndex(
                name: "IX_ForumUsers_ForumId",
                table: "ForumUser",
                newName: "IX_ForumUser_ForumId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ForumUser",
                table: "ForumUser",
                columns: new[] { "UserId", "ForumId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ForumUser_Forums_ForumId",
                table: "ForumUser",
                column: "ForumId",
                principalTable: "Forums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
