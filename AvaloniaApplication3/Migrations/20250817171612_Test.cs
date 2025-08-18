using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvaloniaApplication3.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentIndex",
                table: "QuizAttempts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "QuizAttempts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_UserId_QuizId_Status",
                table: "QuizAttempts",
                columns: new[] { "UserId", "QuizId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_UserId_QuizId_Status",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "CurrentIndex",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "QuizAttempts");
        }
    }
}
