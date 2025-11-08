using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCloud_v42.Migrations
{
    /// <inheritdoc />
    public partial class some_fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course Files_Course Element_Course element1",
                table: "Course Files");

            migrationBuilder.DropIndex(
                name: "IX_Course Files_Course element1",
                table: "Course Files");

            migrationBuilder.DropColumn(
                name: "Course element1",
                table: "Course Files");

            migrationBuilder.RenameColumn(
                name: "Course element",
                table: "Course Files",
                newName: "CourseElement");

            migrationBuilder.CreateIndex(
                name: "IX_Course Files_CourseElement",
                table: "Course Files",
                column: "CourseElement");

            migrationBuilder.AddForeignKey(
                name: "FK_Course Files_Course Element_CourseElement",
                table: "Course Files",
                column: "CourseElement",
                principalTable: "Course Element",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course Files_Course Element_CourseElement",
                table: "Course Files");

            migrationBuilder.DropIndex(
                name: "IX_Course Files_CourseElement",
                table: "Course Files");

            migrationBuilder.RenameColumn(
                name: "CourseElement",
                table: "Course Files",
                newName: "Course element");

            migrationBuilder.AddColumn<int>(
                name: "Course element1",
                table: "Course Files",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Course Files_Course element1",
                table: "Course Files",
                column: "Course element1");

            migrationBuilder.AddForeignKey(
                name: "FK_Course Files_Course Element_Course element1",
                table: "Course Files",
                column: "Course element1",
                principalTable: "Course Element",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
