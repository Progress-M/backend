using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Main.PostgreSQL
{
    public class KindContext : DbContext
    {
        public KindContext(DbContextOptions<KindContext> options) : base(options) { }

        public DbSet<Company> Company { get; set; }
    }

    public class Company
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
    }
}