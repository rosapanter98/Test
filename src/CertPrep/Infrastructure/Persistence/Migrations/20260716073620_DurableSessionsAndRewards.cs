using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CertPrep.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DurableSessionsAndRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "PracticeSessions",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "Standard");

            migrationBuilder.AddColumn<string>(
                name: "ObjectiveContentKeySnapshot",
                table: "PracticeSessionItems",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "NOCASE");

            migrationBuilder.Sql("""
                UPDATE PracticeSessionItems
                SET ObjectiveContentKeySnapshot = COALESCE(
                    (
                        SELECT ExamObjectives.ContentKey
                        FROM Questions
                        INNER JOIN ExamObjectives ON ExamObjectives.Id = Questions.ExamObjectiveId
                        WHERE Questions.Id = PracticeSessionItems.SourceQuestionId
                    ),
                    'legacy-' || SourceExamId || '-' || substr(hex(ObjectiveName), 1, 72)
                )
                WHERE ObjectiveContentKeySnapshot = '';
                """);

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
                name: "RewardLedgerEntries");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "PracticeSessions");

            migrationBuilder.DropColumn(
                name: "ObjectiveContentKeySnapshot",
                table: "PracticeSessionItems");
        }
    }
}
