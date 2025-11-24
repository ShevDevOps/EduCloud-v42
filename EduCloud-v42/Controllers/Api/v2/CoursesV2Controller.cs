using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduCloud_v42.Models;

namespace EduCloud_v42.Controllers.Api.v2
{
    // DTO для версії 2, що включає лічильник елементів
    public class CourseV2Dto
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ElementsCount { get; set; } // Нове поле у V2
        public string ApiVersion { get; set; } = "2.0";
    }

    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/courses")]
    public class CoursesV2Controller : ControllerBase
    {
        private readonly LearningDbContext _context;

        public CoursesV2Controller(LearningDbContext context)
        {
            _context = context;
        }

        // GET: api/v2/courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseV2Dto>>> GetCourses()
        {
            // Використовуємо проекцію для створення DTO
            var courses = await _context.Courses
                .Include(c => c.CourseElements)
                .Select(c => new CourseV2Dto
                {
                    ID = c.ID,
                    Name = c.Name,
                    Description = c.Description,
                    ElementsCount = c.CourseElements.Count // Підрахунок
                })
                .ToListAsync();

            return Ok(courses);
        }

        // GET: api/v2/courses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseV2Dto>> GetCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.CourseElements)
                .Where(c => c.ID == id)
                .Select(c => new CourseV2Dto
                {
                    ID = c.ID,
                    Name = c.Name,
                    Description = c.Description,
                    ElementsCount = c.CourseElements.Count
                })
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }

        // Інші методи (POST, PUT, DELETE) можуть залишатися такими ж або теж приймати DTO.
        // Для демонстрації версійності часто достатньо різниці у GET-запитах.

        [HttpPost]
        public async Task<ActionResult<CourseV2Dto>> PostCourse(CourseV2Dto courseDto)
        {
            var course = new Course
            {
                Name = courseDto.Name,
                Description = courseDto.Description
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Повертаємо створений об'єкт у форматі V2
            courseDto.ID = course.ID;
            courseDto.ElementsCount = 0;

            return CreatedAtAction(nameof(GetCourse), new { id = course.ID, version = "2.0" }, courseDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}