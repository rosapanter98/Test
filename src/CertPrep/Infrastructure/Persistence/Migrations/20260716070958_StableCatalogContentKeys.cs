using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CertPrep.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StableCatalogContentKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_ExamId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_ExamObjectives_ExamId_SortOrder",
                table: "ExamObjectives");

            migrationBuilder.AddColumn<string>(
                name: "ContentKey",
                table: "Questions",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "NOCASE");

            migrationBuilder.AddColumn<string>(
                name: "ContentKey",
                table: "ExamObjectives",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "NOCASE");

            migrationBuilder.Sql("UPDATE Questions SET ContentKey = 'legacy-question-' || Id;");
            migrationBuilder.Sql("UPDATE ExamObjectives SET ContentKey = 'legacy-objective-' || Id;");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ExamId_ContentKey",
                table: "Questions",
                columns: new[] { "ExamId", "ContentKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamObjectives_ExamId_ContentKey",
                table: "ExamObjectives",
                columns: new[] { "ExamId", "ContentKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamObjectives_ExamId_SortOrder",
                table: "ExamObjectives",
                columns: new[] { "ExamId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_ExamId_ContentKey",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_ExamObjectives_ExamId_ContentKey",
                table: "ExamObjectives");

            migrationBuilder.DropIndex(
                name: "IX_ExamObjectives_ExamId_SortOrder",
                table: "ExamObjectives");

            migrationBuilder.DropColumn(
                name: "ContentKey",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ContentKey",
                table: "ExamObjectives");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ExamId",
                table: "Questions",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamObjectives_ExamId_SortOrder",
                table: "ExamObjectives",
                columns: new[] { "ExamId", "SortOrder" },
                unique: true);
        }
    }
}
