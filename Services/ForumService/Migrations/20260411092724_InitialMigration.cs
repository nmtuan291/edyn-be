using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForumService.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Threads_ForumThreadId",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommentVotes",
                table: "CommentVotes");

            migrationBuilder.DropColumn(
                name: "ComentId",
                table: "CommentVotes");

            migrationBuilder.RenameColumn(
                name: "ForumThreadId",
                table: "Tags",
                newName: "ForumThreadEfId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_ForumThreadId",
                table: "Tags",
                newName: "IX_Tags_ForumThreadEfId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommentVotes",
                table: "CommentVotes",
                columns: new[] { "UserId", "CommentId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Threads_ForumThreadEfId",
                table: "Tags",
                column: "ForumThreadEfId",
                principalTable: "Threads",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Threads_ForumThreadEfId",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommentVotes",
                table: "CommentVotes");

            migrationBuilder.RenameColumn(
                name: "ForumThreadEfId",
                table: "Tags",
                newName: "ForumThreadId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_ForumThreadEfId",
                table: "Tags",
                newName: "IX_Tags_ForumThreadId");

            migrationBuilder.AddColumn<Guid>(
                name: "ComentId",
                table: "CommentVotes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommentVotes",
                table: "CommentVotes",
                columns: new[] { "UserId", "ComentId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Threads_ForumThreadId",
                table: "Tags",
                column: "ForumThreadId",
                principalTable: "Threads",
                principalColumn: "Id");
        }
    }
}
