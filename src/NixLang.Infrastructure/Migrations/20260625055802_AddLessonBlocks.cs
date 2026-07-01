using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NixLang.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_exercises_lessons_lesson_id",
                table: "exercises");

            migrationBuilder.DropIndex(
                name: "ix_exercises_lesson_id",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "display_order",
                table: "exercises");

            migrationBuilder.DropColumn(
                name: "lesson_id",
                table: "exercises");

            migrationBuilder.CreateTable(
                name: "lesson_blocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    configuration = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    referenced_exercise_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_blocks", x => x.id);
                    table.ForeignKey(
                        name: "fk_lesson_blocks_exercises_referenced_exercise_id",
                        column: x => x.referenced_exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lesson_blocks_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lesson_blocks_lesson_id",
                table: "lesson_blocks",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_blocks_referenced_exercise_id",
                table: "lesson_blocks",
                column: "referenced_exercise_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lesson_blocks");

            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "lesson_id",
                table: "exercises",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_exercises_lesson_id",
                table: "exercises",
                column: "lesson_id");

            migrationBuilder.AddForeignKey(
                name: "fk_exercises_lessons_lesson_id",
                table: "exercises",
                column: "lesson_id",
                principalTable: "lessons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
