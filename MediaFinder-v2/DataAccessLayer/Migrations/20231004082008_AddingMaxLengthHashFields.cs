using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder_v2.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddingMaxLengthHashFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MD5_Hash",
                table: "FileDetails",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SHA256_Hash",
                table: "FileDetails",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SHA512_Hash",
                table: "FileDetails",
                type: "TEXT",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
