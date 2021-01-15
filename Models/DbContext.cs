using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Main.PostgreSQL
{
    public class KindContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .HasIndex(p => new { p.Email })
                .IsUnique(true);

            modelBuilder.Entity<Company>()
               .HasIndex(p => new { p.INN })
               .IsUnique(true);
        }

        public KindContext(DbContextOptions<KindContext> options) : base(options) { }
        public DbSet<Company> Company { get; set; }
        public DbSet<Offer> Offer { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<OfferUser> OfferUser { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<UserEmailCode> UserEmailCode { get; set; }
        public DbSet<CompanyEmailCode> CompanyEmailCode { get; set; }
    }

    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Company
    {
        public Company() { }
        public Company(CompanyRequest request, ProductCategory productСategory)
        {
            Name = request.name;
            Representative = request.representative;
            Email = request.email;
            INN = request.inn;
            Password = request.password;
            Address = request.address;
            TimeOfWork = request.timeOfWork;
            ProductСategory = productСategory;
            EmailConfirmed = false;
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Representative { get; set; }
        public string Email { get; set; }
        public string INN { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string TimeOfWork { get; set; }
        public bool EmailConfirmed { get; set; }
        public ProductCategory ProductСategory { get; set; }
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
        public User() { }
        public User(UserRequest user)
        {
            Email = user.Email;
            Password = user.Password;
            Name = user.Name;
            isMan = user.isMan;
            EmailConfirmed = false;
            BirthYear = user.BirthYear;
        }

        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool isMan { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime BirthYear { get; set; }
    }

    public class UserEmailCode
    {
        public UserEmailCode() { }

        [Key]
        public int Id { get; set; }
        public string code { get; set; }
        public User user { get; set; }
    }

    public class CompanyEmailCode
    {
        public CompanyEmailCode() { }

        [Key]
        public int Id { get; set; }
        public string code { get; set; }
        public Company company { get; set; }
    }

    public class OfferUser
    {
        public OfferUser() { }
        public OfferUser(Offer offer, User user)
        {
            Offer = offer;
            User = user;
            Date = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Offer Offer { get; set; }
        public User User { get; set; }
    }
}