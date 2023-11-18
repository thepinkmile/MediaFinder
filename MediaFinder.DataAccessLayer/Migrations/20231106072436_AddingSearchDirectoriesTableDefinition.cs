using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddingSearchDirectoriesTableDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchDirectory_SearchSettings_SettingsId",
                table: "SearchDirectory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchDirectory",
                table: "SearchDirectory");

            migrationBuilder.RenameTable(
                name: "SearchDirectory",
                newName: "SearchDirectories");

            migrationBuilder.RenameIndex(
                name: "IX_SearchDirectory_SettingsId",
                table: "SearchDirectories",
                newName: "IX_SearchDirectories_SettingsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchDirectories",
                table: "SearchDirectories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchDirectories_SearchSettings_SettingsId",
                table: "SearchDirectories",
                column: "SettingsId",
                principalTable: "SearchSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchDirectories_SearchSettings_SettingsId",
                table: "SearchDirectories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchDirectories",
                table: "SearchDirectories");

            migrationBuilder.RenameTable(
                name: "SearchDirectories",
                newName: "SearchDirectory");

            migrationBuilder.RenameIndex(
                name: "IX_SearchDirectories_SettingsId",
                table: "SearchDirectory",
                newName: "IX_SearchDirectory_SettingsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchDirectory",
                table: "SearchDirectory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchDirectory_SearchSettings_SettingsId",
                table: "SearchDirectory",
                column: "SettingsId",
                principalTable: "SearchSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
