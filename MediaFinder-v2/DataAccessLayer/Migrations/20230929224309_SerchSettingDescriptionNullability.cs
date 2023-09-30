using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder_v2.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class SerchSettingDescriptionNullability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchDirectories");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SearchSettings",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "SearchDirectory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    SettingsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchDirectory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchDirectory_SearchSettings_SettingsId",
                        column: x => x.SettingsId,
                        principalTable: "SearchSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchDirectory_SettingsId",
                table: "SearchDirectory",
                column: "SettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchDirectory");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SearchSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SearchDirectories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SettingsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchDirectories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchDirectories_SearchSettings_SettingsId",
                        column: x => x.SettingsId,
                        principalTable: "SearchSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchDirectories_SettingsId",
                table: "SearchDirectories",
                column: "SettingsId");
        }
    }
}
