using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduCloud_v42.Models;

namespace EduCloud_v42.Controllers
{
    public class CourseElementsController : Controller
    {
        private readonly LearningDbContext _context;

        public CourseElementsController(LearningDbContext context)
        {
            _context = context;
        }

        // GET: CourseElements?courseId=5
        public async Task<IActionResult> Index(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound("Course not found");
            }

            ViewData["CourseId"] = courseId;
            ViewData["CourseName"] = course.Name;

            var courseElements = _context.CourseElements
                .Where(ce => ce.CourseId == courseId);

            return View(await courseElements.ToListAsync());
        }

        // GET: CourseElements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseElement = await _context.CourseElements
                .Include(c => c.Course)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (courseElement == null)
            {
                return NotFound();
            }

            return View(courseElement);
        }

        // GET: CourseElements/Create?courseId=5
        public IActionResult Create(int courseId)
        {
            // Передаємо CourseId у View, щоб при POST-запиті він був включений у форму
            ViewData["CourseId"] = courseId;
            return View(new CourseElement { CourseId = courseId });
        }

        // POST: CourseElements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Type,CourseId")] CourseElement courseElement)
        {
            if (ModelState.IsValid)
            {
                _context.Add(courseElement);
                await _context.SaveChangesAsync();
                // Повертаємось до Курсу
                return RedirectToAction("Details", "Courses", new { id = courseElement.CourseId });
            }
            return View(courseElement);
        }

        // GET: CourseElements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseElement = await _context.CourseElements.FindAsync(id);
            if (courseElement == null)
            {
                return NotFound();
            }
            return View(courseElement);
        }

        // POST: CourseElements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Description,Type,CourseId")] CourseElement courseElement)
        {
            if (id != courseElement.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(courseElement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.CourseElements.Any(e => e.ID == courseElement.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Повертаємось до списку елементів цього ж курсу
                return RedirectToAction(nameof(Index), new { courseId = courseElement.CourseId });
            }
            return View(courseElement);
        }

        // GET: CourseElements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseElement = await _context.CourseElements
                .Include(c => c.Course)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (courseElement == null)
            {
                return NotFound();
            }

            return View(courseElement);
        }

        // POST: CourseElements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var courseElement = await _context.CourseElements.FindAsync(id);
            int courseId = 0;

            if (courseElement != null)
            {
                courseId = courseElement.CourseId; // Зберігаємо ID курсу для редиректу
                _context.CourseElements.Remove(courseElement);
                await _context.SaveChangesAsync();
            }

            // Повертаємось до списку елементів цього ж курсу
            return RedirectToAction(nameof(Index), new { courseId = courseId });
        }
    }
}