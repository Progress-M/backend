using System;
using System.Collections.Generic;
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
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<EmailCode> EmailCode { get; set; }
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
            AvatarName = "";
            PlayerId = request.playerId;
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
        public string PlayerId { get; set; }
        public ProductCategory ProductСategory { get; set; }
        public string AvatarName { get; set; }
    }

    public class Offer
    {
        public Offer() { }
        public Offer(OfferRequest request, Company company)
        {
            Text = request.text;
            TimeStart = request.timeStart;
            TimeEnd = request.timeEnd;
            Percentage = request.percentage;
            Company = company;
            ImageName = "";
            CreateDate = DateTime.UtcNow;
            ForMan = request.forMan;
            ForWoman = request.forWoman;
            SendingTime = request.sendingTime;
            UpperAgeLimit = request.UpperAgeLimit;
            LowerAgeLimit = request.LowerAgeLimit;
        }

        [Key]
        public int Id { get; set; }
        public int LikeCounter { get; set; }
        public string Text { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime SendingTime { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public Company Company { get; set; }
        public string ImageName { get; set; }
        public int Percentage { get; set; }
        public bool ForMan { get; set; }
        public bool ForWoman { get; set; }
        public int UpperAgeLimit { get; set; }
        public int LowerAgeLimit { get; set; }
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
            AvatarName = "";
            PlayerId = user.playerId;
            Favorites = new HashSet<Company>();
            LikedPosts = new HashSet<Offer>();
        }

        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool isMan { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime BirthYear { get; set; }
        public string AvatarName { get; set; }
        public string PlayerId { get; set; }
        public ICollection<Company> Favorites { get; set; }
        public ICollection<Offer> LikedPosts { get; set; }

    }

    public class EmailCode
    {
        public EmailCode() { }

        [Key]
        public int Id { get; set; }
        public string code { get; set; }
        public string email { get; set; }
    }
}