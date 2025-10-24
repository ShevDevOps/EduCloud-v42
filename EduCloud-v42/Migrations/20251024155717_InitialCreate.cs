using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCloud_v42.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Course Element",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    type = table.Column<string>(type: "TEXT", nullable: false),
                    course = table.Column<int>(type: "INTEGER", nullable: false),
                    course1 = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course Element", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Course Element_Course_course1",
                        column: x => x.course1,
                        principalTable: "Course",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users to Courses",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users to Courses", x => new { x.UserId, x.CourseId });
                    table.ForeignKey(
                        name: "FK_Users to Courses_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users to Courses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Course Files",
                columns: table => new
                {
                    UniqueID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    path = table.Column<string>(type: "TEXT", nullable: false),
                    Courseelement = table.Column<int>(name: "Course element", type: "INTEGER", nullable: false),
                    Courseelement1 = table.Column<int>(name: "Course element1", type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course Files", x => x.UniqueID);
                    table.ForeignKey(
                        name: "FK_Course Files_Course Element_Course element1",
                        column: x => x.Courseelement1,
                        principalTable: "Course Element",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User to Task",
                columns: table => new
                {
                    User = table.Column<int>(type: "INTEGER", nullable: false),
                    Task = table.Column<int>(type: "INTEGER", nullable: false),
                    Mark = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User to Task", x => new { x.User, x.Task });
                    table.ForeignKey(
                        name: "FK_User to Task_Course Element_Task",
                        column: x => x.Task,
                        principalTable: "Course Element",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User to Task_Users_User",
                        column: x => x.User,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Taks Files",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    User = table.Column<int>(type: "INTEGER", nullable: false),
                    Task = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taks Files", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Taks Files_User to Task_User_Task",
                        columns: x => new { x.User, x.Task },
                        principalTable: "User to Task",
                        principalColumns: new[] { "User", "Task" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Course Element_course1",
                table: "Course Element",
                column: "course1");

            migrationBuilder.CreateIndex(
                name: "IX_Course Files_Course element1",
                table: "Course Files",
                column: "Course element1");

            migrationBuilder.CreateIndex(
                name: "IX_Taks Files_User_Task",
                table: "Taks Files",
                columns: new[] { "User", "Task" });

            migrationBuilder.CreateIndex(
                name: "IX_User to Task_Task",
                table: "User to Task",
                column: "Task");

            migrationBuilder.CreateIndex(
                name: "IX_Users to Courses_CourseId",
                table: "Users to Courses",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Course Files");

            migrationBuilder.DropTable(
                name: "Taks Files");

            migrationBuilder.DropTable(
                name: "Users to Courses");

            migrationBuilder.DropTable(
                name: "User to Task");

            migrationBuilder.DropTable(
                name: "Course Element");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Course");
        }
    }
}
