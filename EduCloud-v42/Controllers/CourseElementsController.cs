using EduCloud_v42.Models;
using EduCloud_v42.Srevices.Loginer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduCloud_v42.Controllers
{
    public class CourseElementsController : Controller
    {
        private readonly LearningDbContext _context;
        private readonly ILoginer _loginer;

        public CourseElementsController(LearningDbContext context, ILoginer loginer)
        {
            _context = context;
            _loginer = loginer;
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

            ViewBag.User = _loginer.getUser(HttpContext);
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

            User? user = _loginer.getUser(HttpContext);
            List<TaskFile> taskFiles = new List<TaskFile>();
            if (user != null)
            {
                UserTask? ut = user.UserTasks.Where(ut => ut.TaskId == id).FirstOrDefault();
                
                if(ut != null)
                {
                    taskFiles = ut.TaskFiles.ToList();
                }
            }
            



            ViewBag.User = user;
            ViewBag.taskFiles = taskFiles;
            return View(courseElement);
        }

        [HttpPost]
        public IActionResult Submit(int elementId, IEnumerable<IFormFile> files)
        {
            User? user = _loginer.getUser(HttpContext);

            var courseElement =  _context.CourseElements
                .Include(c => c.Course)
                .FirstOrDefault(m => m.ID == elementId);

            if(courseElement == null)
            {
                return NotFound();
            }

            if (user == null || 
                !user.UserCourses.Any(uc => uc.CourseId == courseElement.CourseId && uc.Role == CourseRole.Student) ||
                user.UserTasks.Any(ut => ut.TaskId == elementId))
            {
                return RedirectToAction("Details", new { elementId });
            }


            _context.UserTasks.Add(new UserTask { TaskId = elementId, UserId = user.ID, Mark=""});

            _context.SaveChanges();

            foreach (var file in files)
            {
                _context.AddTaskFile(file, user.ID, elementId);
            }


            return RedirectToAction("Details", new { id=elementId });
        }


        // GET: CourseElements/Create?courseId=5
        public IActionResult Create(int id)
        {
            // Передаємо CourseId у View, щоб при POST-запиті він був включений у форму
            ViewData["CourseId"] = id;

            User? user = _loginer.getUser(HttpContext);
            if (user == null || !user.UserCourses.Any(uc => uc.CourseId == id && uc.Role == CourseRole.Teacher))
            {
                return NotFound();
            }

            return View(new CourseElement { CourseId = id });
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

            var courseElement = await _context.CourseElements
                .Include(c => c.Course)
                .FirstOrDefaultAsync(m => m.ID == id);


            if (courseElement == null)
            {
                return NotFound();
            }

            User? user = _loginer.getUser(HttpContext);
            if (user == null || !user.UserCourses.Any(uc => uc.CourseId == courseElement.CourseId && uc.Role == CourseRole.Teacher))
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
                // Повертаємось до списку елементів цього ж курсу || Ivan: перенаправив на сторінку Details курсу
                return RedirectToAction("Details", "Courses", new { id = courseElement.CourseId });
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

            User? user = _loginer.getUser(HttpContext);
            if (user == null || !user.UserCourses.Any(uc => uc.CourseId == courseElement.CourseId && uc.Role == CourseRole.Teacher))
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