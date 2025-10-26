using EduCloud_v42.Models;

namespace EduCloud_v42.Srevices.Loginer
{
    public interface ILoginer
    {
        public Task login(HttpContext context, User user);
        public Task logout(HttpContext context);
        public User? getUser(HttpContext context);
    }
}
