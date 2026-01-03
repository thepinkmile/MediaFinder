using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddingDiscoveryRunsConfigIdAndWorkingDirectoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkingDirectory",
                table: "Runs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkingDirectory",
                table: "Runs");
        }
    }
}
