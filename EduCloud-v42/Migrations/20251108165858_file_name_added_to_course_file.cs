using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCloud_v42.Migrations
{
    /// <inheritdoc />
    public partial class file_name_added_to_course_file : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Course Files",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Course Files");
        }
    }
}
