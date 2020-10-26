using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Main.PostgreSQL
{
    public class KindContext : DbContext
    {
        public KindContext(DbContextOptions<KindContext> options) : base(options) { }
        public DbSet<Company> Company { get; set; }
        public DbSet<Offer> Offer { get; set; }
    }


    public class Company
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Offer
    {
        public Offer() { }
        public Offer(string text, DateTime timeStart, Company company)
        {
            Text = text;
            TimeStart = timeStart;
            Company = company;
        }

        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeStart { get; set; }
        public Company Company { get; set; }
    }
}