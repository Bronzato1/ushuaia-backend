using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    public class PostContext : DbContext
    {
        public PostContext(DbContextOptions<PostContext> options) : base(options)
        { }

        public DbSet<PostItem> PostItems { get; set; }
    }
}