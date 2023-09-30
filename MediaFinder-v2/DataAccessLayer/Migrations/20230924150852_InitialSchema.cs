using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder_v2.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "FileDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentPath = table.Column<string>(type: "TEXT", nullable: false),
                    FileNamae = table.Column<string>(type: "TEXT", nullable: false),
                    MD5_Hash = table.Column<string>(type: "TEXT", nullable: false),
                    SHA256_Hash = table.Column<string>(type: "TEXT", nullable: false),
                    SHA512_Hash = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    ShouldExport = table.Column<bool>(type: "INTEGER", nullable: false),
                    FileType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Recursive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExtractArchives = table.Column<bool>(type: "INTEGER", nullable: false),
                    PerformDeepAnalysis = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchDirectories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    SettingsId = table.Column<int>(type: "INTEGER", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "FileDetails");

            migrationBuilder.DropTable(
                name: "SearchDirectories");

            migrationBuilder.DropTable(
                name: "SearchSettings");
        }
    }
}
