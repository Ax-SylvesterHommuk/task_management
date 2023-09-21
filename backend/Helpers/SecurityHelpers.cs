using System.Security.Cryptography;
using System.Text;

namespace task_backend.Helpers
{
    public class SecurityHelpers
    {
        private readonly IConfiguration configuration;

        public SecurityHelpers(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string HashPassword(string password, int saltRounds)
        {
            string salt = GenerateSalt(password, saltRounds);

            string hashedPassword = HashWithSalt(password, salt);

            return hashedPassword;
        }

        private string GenerateSalt(string password, int saltRounds)
        {
            string salt = configuration.GetValue<string>("SecuritySettings:Salt");

            using (Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), saltRounds))
            {
                byte[] saltBytes = rfc2898.GetBytes(16);
                return Convert.ToBase64String(saltBytes);
            }
        }

        private string HashWithSalt(string password, string salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] saltedPasswordBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hash = sha256.ComputeHash(saltedPasswordBytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}