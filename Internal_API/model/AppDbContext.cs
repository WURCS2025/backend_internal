using Internal_API.model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Internal_API.models
{
    public class AppDbContext : DbContext
    {
        const string connectionString = "Host=192.168.12.167;Port=5433;Database=postgres;Username=postgres;Password=password";

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<FileUpload> FileUploads { get; set; }
        public virtual DbSet<UserInfo> UserInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseNpgsql(connectionString);
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileUpload>().ToTable("FileUpload");
        }
    }
}
