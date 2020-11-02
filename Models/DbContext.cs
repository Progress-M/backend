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
        public DbSet<OfferUser> OfferUser { get; set; }
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
        public Offer(OfferRequest request, Company company)
        {
            Text = request.text;
            TimeStart = request.timeStart;
            TimeEnd = request.timeEnd;
            Company = company;
        }

        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public Company Company { get; set; }
    }

    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }

    }

    public class OfferUser
    {
        public OfferUser() { }
        public OfferUser(Offer offer, User user)
        {
            Offer = offer;
            User = user;
        }

        [Key]
        public int Id { get; set; }
        public Offer Offer { get; set; }
        public User User { get; set; }
    }
}