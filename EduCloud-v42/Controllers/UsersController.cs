using EduCloud_v42.Models;
using EduCloud_v42.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace EduCloud_v42.Controllers
{
    public class UsersController : Controller
    {
        private readonly LearningDbContext _context;

        public UsersController(LearningDbContext context)
        {
            _context = context;
        }

        public class UniqueAttribute : ValidationAttribute
        {
            public string PropertyName { get; }

            public UniqueAttribute(string propertyName)
            {
                PropertyName = propertyName;
            }

            //перевірка унікальності текстових полів
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value == null)
                    return ValidationResult.Success;

                var _context = (LearningDbContext)validationContext.GetService(typeof(LearningDbContext))!;
                if (_context == null)
                    throw new InvalidOperationException("DbContext not found in ValidationContext.");

                var dbSet = _context.Set<User>();

                // отримуємо ID редагованого користувача з ViewModel
                var objectInstance = validationContext.ObjectInstance;
                var idProperty = objectInstance.GetType().GetProperty("ID");
                int currentId = idProperty != null ? (int)idProperty.GetValue(objectInstance)! : 0;

                // перевірка унікальності з виключенням поточного запису
                bool exists = dbSet.Any(u => EF.Property<string>(u, PropertyName) == (string)value && u.ID != currentId);

                if (exists)
                    return new ValidationResult(ErrorMessage ?? $"{PropertyName} already exists.");

                return ValidationResult.Success;
            }
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.ID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View(new UserCreateViewModel());
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = viewModel.Username,
                    FullName = viewModel.FullName,
                    Email = viewModel.Email,
                    Phone = viewModel.Phone,
                    Role = viewModel.Role
                };

                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(viewModel.Password));
                    user.PasswordHash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
                }

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            var vm = new UserEditViewModel
            {
                ID = user.ID,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role
            };

            return View(vm);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var user = await _context.Users.FindAsync(viewModel.ID);
            if (user == null)
                return NotFound();

            // Оновлюємо дані
            user.Username = viewModel.Username;
            user.FullName = viewModel.FullName;
            user.Email = viewModel.Email;
            user.Phone = viewModel.Phone;
            user.Role = viewModel.Role;

            // Новий пароль
            if (!string.IsNullOrEmpty(viewModel.NewPassword))
            {
                using var sha256 = SHA256.Create();
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(viewModel.NewPassword));
                user.PasswordHash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.ID == viewModel.ID))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.ID == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}