using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForumService.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns first (Role default = 3 = Member)
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "ForumUsers",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "PermissionOverrides",
                table: "ForumUsers",
                type: "integer",
                nullable: true);

            // Data migration: set Admin (0) for forum creators
            migrationBuilder.Sql(
                """
                UPDATE "ForumUsers" fu
                SET "Role" = 0
                FROM "Forums" f
                WHERE fu."ForumId" = f."Id" AND fu."UserId" = f."CreatorId"
                """);

            // Data migration: set Moderator (2) for users with old Moderator = true who aren't already Admin
            migrationBuilder.Sql(
                """
                UPDATE "ForumUsers"
                SET "Role" = 2
                WHERE "Moderator" = true AND "Role" != 0
                """);

            // Drop old column
            migrationBuilder.DropColumn(
                name: "Moderator",
                table: "ForumUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Moderator",
                table: "ForumUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Restore Moderator flag from Role
            migrationBuilder.Sql(
                """
                UPDATE "ForumUsers"
                SET "Moderator" = true
                WHERE "Role" IN (0, 1, 2)
                """);

            migrationBuilder.DropColumn(
                name: "PermissionOverrides",
                table: "ForumUsers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ForumUsers");
        }
    }
}
