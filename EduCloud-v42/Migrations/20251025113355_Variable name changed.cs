using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCloud_v42.Migrations
{
    /// <inheritdoc />
    public partial class Variablenamechanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UniqueID",
                table: "Course Files",
                newName: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Course Files",
                newName: "UniqueID");
        }
    }
}
