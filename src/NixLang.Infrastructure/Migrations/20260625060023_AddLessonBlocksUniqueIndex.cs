using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NixLang.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonBlocksUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_lesson_blocks_lesson_id",
                table: "lesson_blocks");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_blocks_lesson_id_sequence",
                table: "lesson_blocks",
                columns: new[] { "lesson_id", "sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_lesson_blocks_lesson_id_sequence",
                table: "lesson_blocks");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_blocks_lesson_id",
                table: "lesson_blocks",
                column: "lesson_id");
        }
    }
}
