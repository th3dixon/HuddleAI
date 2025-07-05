using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuddleAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddYouTubeSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                table: "AnalysisRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "YoutubeUrl",
                table: "AnalysisRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "AnalysisRequests");

            migrationBuilder.DropColumn(
                name: "YoutubeUrl",
                table: "AnalysisRequests");
        }
    }
}
