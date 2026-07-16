using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CertPrep.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MixedPracticeSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ExamId",
                table: "PracticeSessions",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "PracticeSessions",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "SingleExam");

            migrationBuilder.AddColumn<string>(
                name: "ExamCodeSnapshot",
                table: "PracticeSessionItems",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamTitleSnapshot",
                table: "PracticeSessionItems",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceExamId",
                table: "PracticeSessionItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE PracticeSessionItems
                SET SourceExamId = (
                        SELECT PracticeSessions.ExamId
                        FROM PracticeSessions
                        WHERE PracticeSessions.Id = PracticeSessionItems.PracticeSessionId),
                    ExamCodeSnapshot = (
                        SELECT PracticeSessions.ExamCodeSnapshot
                        FROM PracticeSessions
                        WHERE PracticeSessions.Id = PracticeSessionItems.PracticeSessionId),
                    ExamTitleSnapshot = (
                        SELECT PracticeSessions.ExamTitleSnapshot
                        FROM PracticeSessions
                        WHERE PracticeSessions.Id = PracticeSessionItems.PracticeSessionId);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "ExamCodeSnapshot",
                table: "PracticeSessionItems",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExamTitleSnapshot",
                table: "PracticeSessionItems",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SourceExamId",
                table: "PracticeSessionItems",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessionItems_SourceExamId",
                table: "PracticeSessionItems",
                column: "SourceExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_PracticeSessionItems_Exams_SourceExamId",
                table: "PracticeSessionItems",
                column: "SourceExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PracticeSessionItems_Exams_SourceExamId",
                table: "PracticeSessionItems");

            migrationBuilder.DropIndex(
                name: "IX_PracticeSessionItems_SourceExamId",
                table: "PracticeSessionItems");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "PracticeSessions");

            migrationBuilder.DropColumn(
                name: "ExamCodeSnapshot",
                table: "PracticeSessionItems");

            migrationBuilder.DropColumn(
                name: "ExamTitleSnapshot",
                table: "PracticeSessionItems");

            migrationBuilder.DropColumn(
                name: "SourceExamId",
                table: "PracticeSessionItems");

            migrationBuilder.AlterColumn<int>(
                name: "ExamId",
                table: "PracticeSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
