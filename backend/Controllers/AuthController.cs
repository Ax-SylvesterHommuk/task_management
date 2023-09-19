using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using task_backend.Models;
using System.Security.Cryptography;
using System.Text;

namespace task_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly string ConnectionString = "Data Source=mydatabase.db;Version=3;";

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] AuthenticationRequest request)
        {
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                if (UserExists(connection, request.Username))
                {
                    return Conflict("Username already exists");
                }

                string hashedPassword = HashPassword(request.Password);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.Username,
                    HashedPassword = hashedPassword
                };

                InsertUser(connection, user);

                return CreatedAtAction("GetUserProfile", new { id = user.Id }, user);
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AuthenticationRequest request)
        {
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                var user = GetUserByUsername(connection, request.Username);

                if (user == null)
                {
                    return Unauthorized("Either username or password is incorrect");
                }

                if (VerifyPassword(request.Password, user.HashedPassword))
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());

                    return Ok("Login successful");
                }

                return Unauthorized("Either username or password is incorrect");
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok("Logged out successfully");
        }

        [HttpGet("profile")]
        public IActionResult GetUserProfile()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Not authenticated");
            }

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                var user = GetUserById(connection, Guid.Parse(userId));

                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(user);
            }
        }

        // Check if a user with the given username exists
        private bool UserExists(SQLiteConnection connection, string username)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                return (long)cmd.ExecuteScalar() > 0;
            }
        }

        // Insert a new user into the Users table
        private void InsertUser(SQLiteConnection connection, User user)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Users (Id, Username, HashedPassword) VALUES (@Id, @Username, @HashedPassword)", connection))
            {
                cmd.Parameters.AddWithValue("@Id", user.Id.ToString());
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@HashedPassword", user.HashedPassword);
                cmd.ExecuteNonQuery();
            }
        }

        // Retrieve a user by their username
        private User GetUserByUsername(SQLiteConnection connection, string username)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Users WHERE Username = @Username", connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = Guid.Parse(reader["Id"].ToString()),
                            Username = reader["Username"].ToString(),
                            HashedPassword = reader["HashedPassword"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        // Retrieve a user by their ID
        private User GetUserById(SQLiteConnection connection, Guid userId)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Users WHERE Id = @Id", connection))
            {
                cmd.Parameters.AddWithValue("@Id", userId.ToString());
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = Guid.Parse(reader["Id"].ToString()),
                            Username = reader["Username"].ToString(),
                            HashedPassword = reader["HashedPassword"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        // SHA256 hashing the password
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return string.Equals(hashedInput, hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}