using EduCloud_v42.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduCloud_v42.Srevices.Loginer
{
    public class CookieLoginer : ILoginer
    {
        LearningDbContext db;
        public CookieLoginer(LearningDbContext db)
        {
            this.db = db;
        }

        public User? getUser(HttpContext context)
        {
            if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                return null;
            }
            Claim? idClaim = context.User.FindFirst("id");
            if (idClaim == null)
            {
                return null;
            }

            int id;
            bool ok = int.TryParse(idClaim.Value, out id);
            if (!ok)
            {
                return null;
            }

            User? user = db.Users.Where(u => u.ID == id).Include(u => u.UserCourses).Include(u => u.UserTasks).ThenInclude(ut => ut.TaskFiles).FirstOrDefault();
            return user;
        }

        public async Task login(HttpContext context, User user)
        {
            if (string.IsNullOrEmpty(user.Username))
            {
                return;
            }
            List<Claim> claims = new List<Claim> {  new Claim(ClaimTypes.Name, user.Username),
                                                    new Claim("id", user.ID.ToString()),
                                                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.ToString()) };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        }

        public async Task logout(HttpContext context)
        {
            await context.SignOutAsync();
        }
    }
}
