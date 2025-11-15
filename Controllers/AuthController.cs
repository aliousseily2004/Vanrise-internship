using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class AuthController : ApiController
    {
        // Simulated user database (replace with your actual database logic)
        private static List<User> users = new List<User>
        {
            new User {
                Username = "Ali Ousseily",
                PasswordHash = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918" // Store the hash of the password
            }
        };

        [HttpPost]
        [Route("api/Auth/login")]
        public IHttpActionResult Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null ||
                string.IsNullOrEmpty(loginRequest.Username) ||
                string.IsNullOrEmpty(loginRequest.Password))
            {
                return Ok(new { isAuthenticated = false, message = "Invalid login request" });
            }

            // DO NOT hash again — compare directly
            var user = users.FirstOrDefault(u =>
                u.Username == loginRequest.Username &&
                u.PasswordHash == loginRequest.Password);

            if (user != null)
            {
                return Ok(new { isAuthenticated = true, message = "Login successful" });
            }
            else
            {
                return Ok(new { isAuthenticated = false, message = "Invalid credentials" });
            }
        }

        [HttpGet]
        [Route("api/Auth/users")]
        public IHttpActionResult GetUsers()
        {
            // Return the list of users (username and password hash)
            return Ok(users.Select(u => new { u.Username, u.PasswordHash }));
        }

        // Method to hash the password using SHA256
        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2")); // Convert byte to hex string
                }
                return builder.ToString();
            }
        }
    }

    // Models for login request and user
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}