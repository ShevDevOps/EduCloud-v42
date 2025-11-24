using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduCloud_v42.Models;

namespace EduCloud_v42.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseElementsApiController : ControllerBase
    {
        private readonly LearningDbContext _context;

        public CourseElementsApiController(LearningDbContext context)
        {
            _context = context;
        }

        // GET: api/CourseElementsApi
        // Можна фільтрувати за courseId: api/CourseElementsApi?courseId=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseElement>>> GetCourseElements([FromQuery] int? courseId)
        {
            if (courseId.HasValue)
            {
                return await _context.CourseElements
                    .Where(ce => ce.CourseId == courseId)
                    .ToListAsync();
            }
            return await _context.CourseElements.ToListAsync();
        }

        // GET: api/CourseElementsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseElement>> GetCourseElement(int id)
        {
            var courseElement = await _context.CourseElements.FindAsync(id);

            if (courseElement == null)
            {
                return NotFound();
            }

            return courseElement;
        }

        // PUT: api/CourseElementsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseElement(int id, CourseElement courseElement)
        {
            if (id != courseElement.ID)
            {
                return BadRequest();
            }

            _context.Entry(courseElement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseElementExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CourseElementsApi
        [HttpPost]
        public async Task<ActionResult<CourseElement>> PostCourseElement(CourseElement courseElement)
        {
            _context.CourseElements.Add(courseElement);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCourseElement", new { id = courseElement.ID }, courseElement);
        }

        // DELETE: api/CourseElementsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseElement(int id)
        {
            var courseElement = await _context.CourseElements.FindAsync(id);
            if (courseElement == null)
            {
                return NotFound();
            }

            _context.CourseElements.Remove(courseElement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseElementExists(int id)
        {
            return _context.CourseElements.Any(e => e.ID == id);
        }
    }
}