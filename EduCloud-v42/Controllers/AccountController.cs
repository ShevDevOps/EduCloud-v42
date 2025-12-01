using EduCloud_v42.Models;
using EduCloud_v42.ViewModels;
using EduCloud_v42.Srevices.Loginer;
using EduCloud_v42.ViewModels;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth;

namespace EduCloud_v42.Controllers
{
    // ViewModels used by the AccountController actions



    public class AccountController : Controller
    {
        private readonly LearningDbContext _context;
        private readonly ILoginer _loginer;

        public AccountController(LearningDbContext context, ILoginer loginer)
        {
            _context = context;
            _loginer = loginer;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

                // УВАГА: Це проста реалізація хешування. Для реальних проектів використовуйте BCrypt.Net або Identity.
                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    await _loginer.login(HttpContext, user);
                    return RedirectToAction("Index", "Home"); // Перенаправлення на головну сторінку
                }

                ModelState.AddModelError(string.Empty, "Неправильна спроба входу.");
            }
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Користувач з таким іменем вже існує.");
                    return View(model);
                }

                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Цей Email вже зареєстровано.");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = HashPassword(model.Password), // Хешуємо пароль
                    Email = model.Email,
                    FullName = model.FullName,
                    Phone = model.Phone,
                    Role = UserRole.User // За замовчуванням роль "User"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Автоматичний вхід після реєстрації
                await _loginer.login(HttpContext, user);

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto model)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(model.Token);

            // payload.Email, payload.Name, payload.Subject (Google ID)
            // Перевіряємо, чи користувач вже є в БД
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                // Створюємо нового користувача
                user = new User
                {
                    Email = payload.Email,
                    FullName = payload.Name,
                    Username = payload.Email.Split('@')[0],
                    Role = UserRole.User,
                    PasswordHash = "" // або якийсь випадковий
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Логін користувача через твій CookieLoginer
            await _loginer.login(HttpContext, user);

            return Ok(new { success = true });
        }

        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _loginer.logout(HttpContext);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile(int? id)
        {
            if (id == null)
                return NotFound();

            // Завантажуємо користувача разом з UserCourses та Course
            var user = await _context.Users
                .Include(u => u.UserCourses)          // підвантажуємо зв’язок
                    .ThenInclude(uc => uc.Course)     // підвантажуємо курси
                .FirstOrDefaultAsync(u => u.ID == id);

            if (user == null)
                return NotFound();

            // Отримуємо список курсів користувача
            var courses = user.UserCourses.Select(uc => uc.Course).ToList();

            // Передаємо у View через ViewModel або ViewBag
            ViewBag.SubscribedCourses = courses;

            return View("Index", user);
        }

        public async Task<IActionResult> Subscribe(int? CourseId, int? UserId)
        {
            if (CourseId == null || UserId == null)
                return NotFound();

            // Перевіряємо, чи вже є підписка
            bool alreadySubscribed = await _context.UserCourses
                .AnyAsync(uc => uc.UserId == UserId.Value && uc.CourseId == CourseId.Value);

            if (alreadySubscribed)
            {
                // Можна повернути повідомлення або редірект
                TempData["Message"] = "Ви вже підписані на цей курс.";
                return RedirectToAction("Details", "Courses", new { id = CourseId });
            }

            var userCourse = new UserCourse
            {
                CourseId = CourseId.Value,
                UserId = UserId.Value,
                Role = CourseRole.Student
            };

            _context.UserCourses.Add(userCourse);  // додаємо в контекст
            await _context.SaveChangesAsync();      // зберігаємо в БД

            return RedirectToAction("Profile", "Account", new { id = UserId });
        }


        // Простий метод хешування паролю. У реальному проекті використовуйте надійніші бібліотеки.
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}
