using EduCloud_v42.Models;
using EduCloud_v42.ViewModels;
using EduCloud_v42.Srevices.Loginer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            {
                return NotFound();
            }

            var User = await _context.Users.FirstOrDefaultAsync(m => m.ID == id);
            if (User == null)
            {
                return NotFound();
            }

            return View("Index", User);
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
