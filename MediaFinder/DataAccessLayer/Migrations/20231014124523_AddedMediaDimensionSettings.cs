using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder_v2.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddedMediaDimensionSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MinImageHeight",
                table: "SearchSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MinImageWidth",
                table: "SearchSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MinVideoHeight",
                table: "SearchSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MinVideoWidth",
                table: "SearchSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SearchSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MinImageHeight", "MinImageWidth", "MinVideoHeight", "MinVideoWidth" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "SearchSettings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MinImageHeight", "MinImageWidth", "MinVideoHeight", "MinVideoWidth" },
                values: new object[] { 200L, 200L, 300L, 600L });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinImageHeight",
                table: "SearchSettings");

            migrationBuilder.DropColumn(
                name: "MinImageWidth",
                table: "SearchSettings");

            migrationBuilder.DropColumn(
                name: "MinVideoHeight",
                table: "SearchSettings");

            migrationBuilder.DropColumn(
                name: "MinVideoWidth",
                table: "SearchSettings");
        }
    }
}
