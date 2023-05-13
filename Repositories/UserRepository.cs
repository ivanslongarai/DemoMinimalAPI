using DemoMinimalAPI.Models;

namespace DemoMinimalAPI.Repositories
{
    public static class UserRepository
    {
        public static User? Get(string username, string password)
        {
            var users = new List<User>
            {
                new User { Id = 1, UserName = "jose", Password = "jose", Role = "manager"},
                new User { Id = 1, UserName = "joao", Password = "joao", Role = "employee"}
            };

            return users.Where(x => x.UserName?.ToLower() == username.ToLower() && x.Password == password).FirstOrDefault();
        }
    }
}
