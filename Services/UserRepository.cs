using CMCS.web2.Models;
using System.Text.Json;

namespace CMCS.web2.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");
        private List<User> _users;

        public UserRepository()
        {
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Data"));
            _users = LoadUsers();

            // Create default HR user if none exists
            if (!_users.Any(u => u.Role == UserRole.HR))
            {
                var defaultHR = new User
                {
                    Email = "hr@university.com",
                    Name = "HR",
                    Surname = "Admin",
                    HourlyRate = 0,
                    Password = "hr123", // Simple password for demo
                    Role = UserRole.HR
                };
                AddUser(defaultHR);

                // Also create a sample lecturer for testing
                var sampleLecturer = new User
                {
                    Email = "lecturer@university.com",
                    Name = "John",
                    Surname = "Doe",
                    HourlyRate = 50.00m,
                    Password = "lecturer123",
                    Role = UserRole.Lecturer
                };
                AddUser(sampleLecturer);
            }
        }
        private List<User> LoadUsers()
        {
            if (!File.Exists(_filePath))
                return new List<User>();

            try
            {
                string json = File.ReadAllText(_filePath);
                var users = JsonSerializer.Deserialize<List<User>>(json);
                return users ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        public void SaveChanges()
        {
            string json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public List<User> GetAllUsers() => _users;

        public User? GetUserById(Guid id) => _users.FirstOrDefault(u => u.UserId == id);

        public User? GetUserByEmail(string email) => _users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        public User? AuthenticateUser(string email, string password)
        {
            var user = GetUserByEmail(email);
            // In production, use proper password hashing
            return user != null && user.Password == password && user.IsActive ? user : null;
        }

        public void AddUser(User user)
        {
            _users.Add(user);
            SaveChanges();
        }

        public void UpdateUser(User user)
        {
            var existing = GetUserById(user.UserId);
            if (existing != null)
            {
                _users.Remove(existing);
                _users.Add(user);
                SaveChanges();
            }
        }

        public void DeleteUser(Guid id)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                user.IsActive = false;
                SaveChanges();
            }
        }
    }
}