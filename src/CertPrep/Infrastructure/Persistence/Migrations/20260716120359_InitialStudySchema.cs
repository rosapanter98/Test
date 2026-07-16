using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CertPrep.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialStudySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, collation: "NOCASE"),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ContentVersion = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamObjectives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExamId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamObjectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamObjectives_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PracticeSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExamId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExamCodeSnapshot = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    ExamTitleSnapshot = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Scope = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Purpose = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CurrentItemIndex = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeSessions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExamId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExamObjectiveId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    Prompt = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    SourceName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SourceUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_ExamObjectives_ExamObjectiveId",
                        column: x => x.ExamObjectiveId,
                        principalTable: "ExamObjectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Questions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PracticeSessionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PracticeSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceQuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceExamId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExamCodeSnapshot = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    ExamTitleSnapshot = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    ObjectiveContentKeySnapshot = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    ObjectiveName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Prompt = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    SourceName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SourceUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeSessionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeSessionItems_Exams_SourceExamId",
                        column: x => x.SourceExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PracticeSessionItems_PracticeSessions_PracticeSessionId",
                        column: x => x.PracticeSessionId,
                        principalTable: "PracticeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RewardLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PracticeSessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    PracticeSessionItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExamCode = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    ObjectiveContentKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, collation: "NOCASE"),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "TEXT", maxLength: 240, nullable: false, collation: "NOCASE"),
                    RuleVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 240, nullable: false),
                    AwardedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardLedgerEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardLedgerEntries_PracticeSessions_PracticeSessionId",
                        column: x => x.PracticeSessionId,
                        principalTable: "PracticeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerChoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerChoices_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PracticeSessionChoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PracticeSessionItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceAnswerChoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSelected = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeSessionChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeSessionChoices_PracticeSessionItems_PracticeSessionItemId",
                        column: x => x.PracticeSessionItemId,
                        principalTable: "PracticeSessionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerChoices_QuestionId_SortOrder",
                table: "AnswerChoices",
                columns: new[] { "QuestionId", "SortOrder" },
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

            migrationBuilder.CreateIndex(
                name: "IX_Exams_Provider_Code",
                table: "Exams",
                columns: new[] { "Provider", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessionChoices_PracticeSessionItemId_SortOrder",
                table: "PracticeSessionChoices",
                columns: new[] { "PracticeSessionItemId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessionItems_PracticeSessionId_OrderIndex",
                table: "PracticeSessionItems",
                columns: new[] { "PracticeSessionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessionItems_SourceExamId",
                table: "PracticeSessionItems",
                column: "SourceExamId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessions_ExamId",
                table: "PracticeSessions",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticeSessions_StartedAt",
                table: "PracticeSessions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ExamId_ContentKey",
                table: "Questions",
                columns: new[] { "ExamId", "ContentKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ExamObjectiveId",
                table: "Questions",
                column: "ExamObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardLedgerEntries_IdempotencyKey",
                table: "RewardLedgerEntries",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RewardLedgerEntries_PracticeSessionId_RuleVersion",
                table: "RewardLedgerEntries",
                columns: new[] { "PracticeSessionId", "RuleVersion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerChoices");

            migrationBuilder.DropTable(
                name: "PracticeSessionChoices");

            migrationBuilder.DropTable(
                name: "RewardLedgerEntries");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "PracticeSessionItems");

            migrationBuilder.DropTable(
                name: "ExamObjectives");

            migrationBuilder.DropTable(
                name: "PracticeSessions");

            migrationBuilder.DropTable(
                name: "Exams");
        }
    }
}
