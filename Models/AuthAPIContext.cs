using Microsoft.EntityFrameworkCore;
namespace AuthAPI.Models
{
    // the MyContext class representing a session with our MySQL 
    // database allowing us to query for or save data
    public class AuthAPIContext : DbContext
    {
        public AuthAPIContext(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }
}