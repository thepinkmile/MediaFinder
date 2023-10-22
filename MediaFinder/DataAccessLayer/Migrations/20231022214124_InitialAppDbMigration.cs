using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MediaFinder_v2.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialAppDbMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentPath = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    MD5_Hash = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    SHA256_Hash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SHA512_Hash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    ShouldExport = table.Column<bool>(type: "INTEGER", nullable: false),
                    FileType = table.Column<int>(type: "INTEGER", nullable: false),
                    Extracted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", nullable: false)
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
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Recursive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExtractArchives = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExtractionDepth = table.Column<int>(type: "INTEGER", nullable: true),
                    PerformDeepAnalysis = table.Column<bool>(type: "INTEGER", nullable: false),
                    MinImageWidth = table.Column<long>(type: "INTEGER", nullable: true),
                    MinImageHeight = table.Column<long>(type: "INTEGER", nullable: true),
                    MinVideoWidth = table.Column<long>(type: "INTEGER", nullable: true),
                    MinVideoHeight = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    FileDetailsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileProperties_FileDetails_FileDetailsId",
                        column: x => x.FileDetailsId,
                        principalTable: "FileDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.InsertData(
                table: "SearchSettings",
                columns: new[] { "Id", "Description", "ExtractArchives", "ExtractionDepth", "MinImageHeight", "MinImageWidth", "MinVideoHeight", "MinVideoWidth", "Name", "PerformDeepAnalysis", "Recursive" },
                values: new object[,]
                {
                    { 1, null, false, null, null, null, null, null, "Default", false, true },
                    { 2, null, true, 5, 200L, 200L, 300L, 600L, "Testing", true, true }
                });

            migrationBuilder.InsertData(
                table: "SearchDirectory",
                columns: new[] { "Id", "Path", "SettingsId" },
                values: new object[,]
                {
                    { 1, "C:\\Users\\User\\Pictures", 1 },
                    { 2, "C:\\TEMP\\Source", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileProperties_FileDetailsId",
                table: "FileProperties",
                column: "FileDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDirectory_SettingsId",
                table: "SearchDirectory",
                column: "SettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileProperties");

            migrationBuilder.DropTable(
                name: "SearchDirectory");

            migrationBuilder.DropTable(
                name: "FileDetails");

            migrationBuilder.DropTable(
                name: "SearchSettings");
        }
    }
}
