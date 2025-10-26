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
    // Керування зв'язком User-Course
    public class EnrollmentController : Controller
    {
        private readonly LearningDbContext _context;

        public EnrollmentController(LearningDbContext context)
        {
            _context = context;
        }

        // Список курсів, на які записаний конкретний користувач
        // GET: Enrollment/CoursesForUser/5
        public async Task<IActionResult> CoursesForUser(int? userId)
        {
            if (userId == null) return BadRequest("User ID is required.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");

            ViewData["UserName"] = user.Username;

            var enrollments = await _context.UserCourses
                .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Course)
                .ToListAsync();

            return View(enrollments);
        }

        // Список користувачів, які записані на конкретний курс
        // GET: Enrollment/UsersInCourse/5
        public async Task<IActionResult> UsersInCourse(int? courseId)
        {
            if (courseId == null) return BadRequest("Course ID is required.");

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound("Course not found.");

            ViewData["CourseName"] = course.Name;
            ViewData["CourseId"] = course.ID;

            var enrollments = await _context.UserCourses
                .Where(uc => uc.CourseId == courseId)
                .Include(uc => uc.User)
                .ToListAsync();

            return View(enrollments);
        }

        // GET: Форма для додавання користувача на курс
        // GET: Enrollment/EnrollUser?courseId=5
        public IActionResult EnrollUser(int courseId)
        {
            var course = _context.Courses.Find(courseId);
            if (course == null) return NotFound();

            ViewData["CourseName"] = course.Name;
            ViewBag.UserId = new SelectList(_context.Users, "ID", "Name");

            return View(new UserCourse { CourseId = courseId });
        }

        // POST: Запис користувача на курс
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollUser([Bind("UserId,CourseId,Role")] UserCourse userCourse)
        {
            // Перевіряємо, чи такий запис вже існує
            var existing = await _context.UserCourses
                .FindAsync(userCourse.UserId, userCourse.CourseId);

            if (existing == null && ModelState.IsValid)
            {
                _context.Add(userCourse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(UsersInCourse), new { courseId = userCourse.CourseId });
            }

            // Якщо щось пішло не так, повертаємо форму
            ViewBag.UserId = new SelectList(_context.Users, "ID", "Name", userCourse.UserId);
            return View(userCourse);
        }


        // POST: Видалення користувача з курсу
        // POST: Enrollment/Unenroll?userId=1&courseId=5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(int userId, int courseId)
        {
            var userCourse = await _context.UserCourses.FindAsync(userId, courseId);
            if (userCourse == null)
            {
                return NotFound();
            }

            _context.UserCourses.Remove(userCourse);
            await _context.SaveChangesAsync();

            // Повертаємось на сторінку курсу
            return RedirectToAction(nameof(UsersInCourse), new { courseId = courseId });
        }
    }
}