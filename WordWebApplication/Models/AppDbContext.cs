using Microsoft.EntityFrameworkCore;

namespace WordWebApplication.Models
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<FileModel> Files { get; set; }
        public DbSet<ImgModel> Imgs { get; set; }
        
    }
}