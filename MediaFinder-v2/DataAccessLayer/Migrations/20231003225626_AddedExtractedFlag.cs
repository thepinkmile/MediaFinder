using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder_v2.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddedExtractedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Extracted",
                table: "FileDetails",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Extracted",
                table: "FileDetails");
        }
    }
}
