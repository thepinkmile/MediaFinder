using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaFinder.DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddingDiscoveryRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDateTime = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Runs_SearchSettings_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "SearchSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Runs_ConfigurationId",
                table: "Runs",
                column: "ConfigurationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Runs");
        }
    }
}
