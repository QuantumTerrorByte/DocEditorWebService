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
        public DbSet<DiagramData> DiagramData { get; set; }
         
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DiagramData>(entity =>
            {
                entity.Property(e => e.DiagramName).HasMaxLength(50);
            });
        }
    }
}