using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using task_backend.Models;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using task_backend.Data;
using Microsoft.AspNetCore.Cors;

namespace task_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowLocalhost")]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseContext _db;

        public AuthController(DatabaseContext db)
        {
            _db = db;
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] AuthenticationRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Username and password cannot be empty.");
            }
            
            using (MySqlConnection connection = _db.GetConnection())
            {
                connection.Open();

                if (UserExists(connection, request.Username))
                {
                    return Conflict("Account already exists");
                }

                string hashedPassword = HashPassword(request.Password);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.Username,
                    HashedPassword = hashedPassword
                };

                InsertUser(connection, user);

                HttpContext.Session.SetString("UserId", user.Id.ToString());

                return CreatedAtAction("GetUserProfile", new { id = user.Id }, user);
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AuthenticationRequest request)
        {
            using (MySqlConnection connection = _db.GetConnection())
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

            using (MySqlConnection connection = _db.GetConnection())
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
        private bool UserExists(MySqlConnection connection, string username)
        {
            using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                return (long)cmd.ExecuteScalar() > 0;
            }
        }

        // Insert a new user into the Users table
        private void InsertUser(MySqlConnection connection, User user)
        {
            using (MySqlCommand cmd = new MySqlCommand("INSERT INTO Users (Id, Username, HashedPassword) VALUES (@Id, @Username, @HashedPassword)", connection))
            {
                cmd.Parameters.AddWithValue("@Id", user.Id.ToString());
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@HashedPassword", user.HashedPassword);
                cmd.ExecuteNonQuery();
            }
        }

        // Retrieve a user by their username
        private User GetUserByUsername(MySqlConnection connection, string username)
        {
            using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM Users WHERE Username = @Username", connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                using (MySqlDataReader reader = cmd.ExecuteReader())
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
        private User GetUserById(MySqlConnection connection, Guid userId)
        {
            using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM Users WHERE Id = @Id", connection))
            {
                cmd.Parameters.AddWithValue("@Id", userId.ToString());
                using (MySqlDataReader reader = cmd.ExecuteReader())
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