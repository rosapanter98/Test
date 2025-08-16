using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvaloniaApplication3.Migrations
{
    /// <inheritdoc />
    public partial class VetIkkje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "Answers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUserCorrect",
                table: "Answers",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "IsUserCorrect",
                table: "Answers");
        }
    }
}
