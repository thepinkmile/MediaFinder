using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder_v2.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddingExtendedFilePropertiesAndExtractionDepthSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExtractionDepth",
                table: "SearchSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FileProperty",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    FileDetailsId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileProperty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileProperty_FileDetails_FileDetailsId",
                        column: x => x.FileDetailsId,
                        principalTable: "FileDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileProperty_FileDetailsId",
                table: "FileProperty",
                column: "FileDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileProperty");

            migrationBuilder.DropColumn(
                name: "ExtractionDepth",
                table: "SearchSettings");
        }
    }
}
