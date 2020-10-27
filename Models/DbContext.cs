using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Main.PostgreSQL
{
    public class KindContext : DbContext
    {
        public KindContext(DbContextOptions<KindContext> options) : base(options) { }
        public DbSet<Company> Company { get; set; }
        public DbSet<Offer> Offer { get; set; }
        public DbSet<User> User { get; set; }
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
        public Offer(OfferRequest request, List<User> users, Company company)
        {
            Text = request.text;
            TimeStart = request.timeStart;
            TimeEnd = request.timeEnd;
            Users = users;
            Company = company;
        }

        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public ICollection<User> Users { get; set; }
        public Company Company { get; set; }
    }

    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }

    }
}