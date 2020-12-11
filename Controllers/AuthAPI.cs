using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AuthAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Cors;

namespace AuthAPI.Controllers
{
    [ApiController]
    [EnableCors]
    [Route("[controller]")]
    public class APIController : Controller
    {
        private readonly ILogger<APIController> _logger;

        private AuthAPIContext _context;

        public APIController(ILogger<APIController> logger, AuthAPIContext context)
        {
            _logger = logger;
            _context = context;

        }

        [HttpPost("users")]
        public async Task<ActionResult<User>> PostUser([FromBody] User user)
        {
            // If a User exists with provided email
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                // error message
                ModelState.AddModelError("Email", "Email already in use!");
                string[] arr = { "Email already in use" };
                return Conflict(new { title = "Conflict", status = 409, errors = new { Email = arr } });
            };

            // Initializing a PasswordHasher object, providing our User class as its type
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            user.Password = Hasher.HashPassword(user, user.Password);

            // add new user to DB
            User newUser = new User()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, newUser);
        }
        [HttpPost("users/auth")]
        public ActionResult<User> AuthUser([FromBody] LoginRequest userSubmission)
        {
            // If inital ModelState is valid, query for a user with provided email
            var userDocument = _context.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
            // If no user exists with provided email
            if (userDocument == null)
            {
                return StatusCode(403);
            }

            // Initialize hasher object
            var hasher = new PasswordHasher<LoginRequest>();

            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(userSubmission, userDocument.Password, userSubmission.Password);

            // result can be compared to 0 for failure
            if (result == 0)
            {
                return Forbid();
            }

            return Ok(userDocument);
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
    }
}
