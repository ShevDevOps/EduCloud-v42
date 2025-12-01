using EduCloud_v42.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCloud_v42.Controllers
{
    public class SubmissionsController : Controller
    {
        private readonly LearningDbContext _context;

        public SubmissionsController(LearningDbContext context)
        {
            _context = context;
        }

        // GET: Submissions/ViewSubmissions?taskId=1
        public async Task<IActionResult> ViewSubmissions(int taskId)
        {
            var task = await _context.CourseElements.FindAsync(taskId);
            if (task == null || task.Type != CourseElementType.Task)
            {
                return NotFound("Task not found.");
            }

            ViewData["TaskName"] = task.Name;
            ViewData["TaskId"] = task.ID;

            var submissions = await _context.UserTasks
                .Where(ut => ut.TaskId == taskId)
                .Include(ut => ut.User)
                .ToListAsync();

            return View(submissions);
        }

        // Сторінка "здачі" роботи (для студента)
        // GET: Submissions/Submit?taskId=1&userId=1
        public async Task<IActionResult> Submit(int taskId, int userId)
        {
            var task = await _context.CourseElements.FindAsync(taskId);
            if (task == null) return NotFound("Task not found.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");

            var submission = await _context.UserTasks
                .FirstOrDefaultAsync(ut => ut.TaskId == taskId && ut.UserId == userId);

            if (submission == null)
            {
                submission = new UserTask { TaskId = taskId, UserId = userId, Mark = "Not submitted" };
            }

            ViewData["TaskName"] = task.Name;
            ViewData["UserName"] = user.Username;

            return View(submission);
        }

        // POST: Здача роботи
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit([Bind("UserId,TaskId,Mark")] UserTask userTask)
        {
            var submission = await _context.UserTasks
                .FindAsync(userTask.UserId, userTask.TaskId);

            if (submission == null)
            {
                userTask.Mark = "Submitted";
                _context.Add(userTask);
            }
            else
            {
                submission.Mark = "Resubmitted";
                _context.Update(submission);
            }

            await _context.SaveChangesAsync();

            var task = await _context.CourseElements.FindAsync(userTask.TaskId);
            return RedirectToAction("Index", "CourseElements", new { courseId = task.CourseId });
        }


        // GET: Submissions/Grade?taskId=1&userId=1
        public async Task<IActionResult> Grade(int taskId, int userId)
        {
            var submission = await _context.UserTasks
                .Include(ut => ut.User)
                .Include(ut => ut.Task)
                .FirstOrDefaultAsync(ut => ut.TaskId == taskId && ut.UserId == userId);

            if (submission == null)
            {
                var user = await _context.Users.FindAsync(userId);
                var task = await _context.CourseElements.FindAsync(taskId);
                if (user == null || task == null) return NotFound();

                submission = new UserTask { UserId = userId, TaskId = taskId, User = user, Task = task, Mark = "Not submitted" };
            }

            return View(submission);
        }

        // POST: Оцінювання роботи
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int taskId, int userId, [Bind("UserId,TaskId,Mark")] UserTask userTask)
        {
            if (taskId != userTask.TaskId || userId != userTask.UserId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var submission = await _context.UserTasks.FindAsync(userId, taskId);
                if (submission == null)
                {
                    _context.Add(userTask);
                }
                else
                {
                    submission.Mark = userTask.Mark;
                    _context.Update(submission);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ViewSubmissions), new { taskId = taskId });
            }
            return View(userTask);
        }
    }
}