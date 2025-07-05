using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuddleAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalysisRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sport = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AnalysisTopic = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: true),
                    Overview = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AreasForImprovement = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DetailedImprovementPlan = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_CreatedAt_DESC",
                table: "AnalysisRequests",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_IsProcessed",
                table: "AnalysisRequests",
                column: "IsProcessed",
                filter: "[IsProcessed] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRequests_Sport_CreatedAt",
                table: "AnalysisRequests",
                columns: new[] { "Sport", "CreatedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisRequests");
        }
    }
}
