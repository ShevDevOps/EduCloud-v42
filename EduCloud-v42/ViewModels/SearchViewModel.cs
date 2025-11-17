using EduCloud_v42.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System;

namespace EduCloud_v42.ViewModels
{
    public class SearchViewModel
    {
        // --- Параметри Пошуку ---

        // (c.i) Пошук по даті
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // (c.ii) Пошук по списку елементів (будемо шукати по курсах)
        public int? SelectedCourseId { get; set; }
        public SelectList? CourseList { get; set; } // Для заповнення dropdown

        // (c.iii) Пошук по початку/кінцю
        public string? FileNameStartsWith { get; set; } // <-- ОНОВЛЕНО (було FileNameContains)
        public string? FileNameEndsWith { get; set; }

        // --- Результати Пошуку ---
        public List<CourseElement> Results { get; set; } = new List<CourseElement>();
    }
}