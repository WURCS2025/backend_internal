using System;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Internal_API.models;

namespace Internal_API.Tests
{
    internal class MyDbContext : DbContext
    {
        public DbSet<FileUpload> FileUploads { get; set; }
        const string connectionString = "Host=localhost;Port=5433;Database=postgres;Username=postgres;Password=password";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }

    }
    [TestClass]
    public class PostgresConnectionTest
    {

        private MyDbContext context;

        static string GenerateRandomText(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }
        [TestMethod]
        public void InsertARecord()
        {
            FileUpload testRecord = new FileUpload { id = Guid.NewGuid().ToString(), filename = "file1.pdf", year = 2023, s3_key = GenerateRandomText(12) };
            using (context = new MyDbContext())
            {
                context.FileUploads.Add(testRecord);
                context.SaveChanges();
                Console.WriteLine("record saved");
                Assert.IsTrue(1 == 1, "Record saving test succeeded");
            }
        }
    }
}


