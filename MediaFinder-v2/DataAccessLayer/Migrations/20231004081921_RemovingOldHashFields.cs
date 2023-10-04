using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder_v2.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemovingOldHashFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MD5_Hash",
                table: "FileDetails");

            migrationBuilder.DropColumn(
                name: "SHA256_Hash",
                table: "FileDetails");

            migrationBuilder.DropColumn(
                name: "SHA512_Hash",
                table: "FileDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MD5_Hash",
                table: "FileDetails",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SHA256_Hash",
                table: "FileDetails",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SHA512_Hash",
                table: "FileDetails",
                type: "TEXT",
                nullable: true);
        }
    }
}
