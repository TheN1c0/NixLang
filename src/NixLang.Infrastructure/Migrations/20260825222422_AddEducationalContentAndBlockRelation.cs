using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NixLang.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEducationalContentAndBlockRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "referenced_educational_content_id",
                table: "lesson_blocks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "educational_contents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    body = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reference_level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_educational_contents", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lesson_blocks_referenced_educational_content_id",
                table: "lesson_blocks",
                column: "referenced_educational_content_id");

            migrationBuilder.AddForeignKey(
                name: "fk_lesson_blocks_educational_contents_referenced_educational_c",
                table: "lesson_blocks",
                column: "referenced_educational_content_id",
                principalTable: "educational_contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_lesson_blocks_educational_contents_referenced_educational_c",
                table: "lesson_blocks");

            migrationBuilder.DropTable(
                name: "educational_contents");

            migrationBuilder.DropIndex(
                name: "ix_lesson_blocks_referenced_educational_content_id",
                table: "lesson_blocks");

            migrationBuilder.DropColumn(
                name: "referenced_educational_content_id",
                table: "lesson_blocks");
        }
    }
}
