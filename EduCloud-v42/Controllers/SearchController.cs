using EduCloud_v42.Models;
using EduCloud_v42.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EduCloud_v42.Controllers
{
    public class SearchController : Controller
    {
        private readonly LearningDbContext _context;

        public SearchController(LearningDbContext context)
        {
            _context = context;
        }

        // GET: Search/Index (відображає форму пошуку)
        public async Task<IActionResult> Index()
        {
            var vm = new SearchViewModel
            {
                // (c.ii) Готуємо список курсів для dropdown
                CourseList = new SelectList(await _context.Courses.ToListAsync(), "ID", "Name")
            };
            return View(vm);
        }

        // POST: Search/Index (виконує пошук та відображає результати)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SearchViewModel vm)
        {
            // (c.iv) Базовий запит з двома JOIN (CourseElements -> Courses ТА CourseElements -> CourseFiles)
            var query_ = _context.CourseElements
                                .Include(ce => ce.Course) // JOIN 1
                                .Include(ce => ce.CourseFiles); // JOIN 2
            var query = query_.ToList();



            // ... (Фільтрація по даті, списку, ElementNameStartsWith) ...
            if (vm.DateFrom.HasValue)
            {
                query = query.Where(ce => ce.CreatedAt >= vm.DateFrom.Value).ToList();
            }
            if (vm.DateTo.HasValue)
            {
                query = query.Where(ce => ce.CreatedAt <= vm.DateTo.Value.AddDays(1)).ToList();
            }

            // (c.ii) Фільтрація по списку (Курси)
            if (vm.SelectedCourseId.HasValue)
            {
                // Було: query = query.Where(ce => vm.SelectedCourseIds.Contains(ce.CourseId));
                query = query.Where(ce => ce.CourseId == vm.SelectedCourseId.Value).ToList();
            }

            // (c.iii + c.iv) Пошук по імені файлу (залежна таблиця CourseFiles)

            // Пошук по ПОЧАТКУ (використовуємо нове поле 'Name')
            if (!string.IsNullOrEmpty(vm.FileNameStartsWith))
            {
                // Замінюємо логіку 'Path.Contains'
                query = query.Where(ce => ce.CourseFiles.Any(cf => cf.Name.StartsWith(vm.FileNameStartsWith))).ToList();
            }

            // (c.iii + c.iv) Пошук по КІНЦЮ (також краще використовувати 'Name')
            if (!string.IsNullOrEmpty(vm.FileNameEndsWith))
            {
                // Замінюємо логіку 'Path.EndsWith'
                query = query.Where(ce => ce.CourseFiles.Any(cf => cf.Name.EndsWith(vm.FileNameEndsWith))).ToList();
            }

            // Виконуємо запит
            vm.Results = query.Distinct().ToList();

            // Повторно заповнюємо SelectList
            vm.CourseList = new SelectList(await _context.Courses.ToListAsync(), "ID", "Name", vm.SelectedCourseId);

            return View(vm);
        }
    }
}