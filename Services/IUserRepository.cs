using CMCS.web2.Models;

namespace CMCS.web2.Services
{
    public interface IUserRepository
    {
        List<User> GetAllUsers();
        User? GetUserById(Guid id);
        User? GetUserByEmail(string email);
        User? AuthenticateUser(string email, string password);
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUser(Guid id);
        void SaveChanges();
    }
}
