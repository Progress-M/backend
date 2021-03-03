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

            modelBuilder.Entity<User>()
                .HasMany(u => u.Stories)
                .WithOne()
                .HasForeignKey("userId");
        }

        public KindContext(DbContextOptions<KindContext> options) : base(options) { }
        public DbSet<Company> Company { get; set; }
        public DbSet<Offer> Offer { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<EmailCode> EmailCode { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<CompanyNotification> CompanyNotification { get; set; }
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
        public Company(CompanyRequest request, ProductCategory productCategory)
        {
            Name = request.name;
            Representative = request.representative;
            Email = request.email;
            INN = request.inn;
            Password = request.password;
            Address = request.address;
            TimeOfWork = request.timeOfWork;
            ProductCategory = productCategory;
            EmailConfirmed = false;
            AvatarName = "";
            PlayerId = request.playerId;
            Phone = request.phone;
            Latitude = request.Latitude;
            Longitude = request.Longitude;
        }

        [Key]
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }
        public string Representative { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string INN { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string TimeOfWork { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PlayerId { get; set; }
        public ProductCategory ProductCategory { get; set; }
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
            Name = user.Name;
            isMan = user.isMan;
            Latitude = user.Latitude;
            Longitude = user.Longitude;
            BirthYear = user.BirthYear;
            AvatarName = "";
            PlayerId = user.playerId;
            Favorites = new HashSet<Company>();
            LikedPosts = new HashSet<Offer>();
            Stories = new HashSet<Offer>();
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool isMan { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime BirthYear { get; set; }
        public string AvatarName { get; set; }
        public string PlayerId { get; set; }
        public ICollection<Company> Favorites { get; set; }
        public ICollection<Offer> LikedPosts { get; set; }
        public ICollection<Offer> Stories { get; set; }

    }

    public class EmailCode
    {
        public EmailCode() { }

        [Key]
        public int Id { get; set; }
        public string code { get; set; }
        public string email { get; set; }
    }

    public class Message
    {
        public Message()
        {
            sendingTime = DateTime.UtcNow;
        }
        public Message(User user, Company company, bool isUserMessage, string text)
        {
            this.user = user;
            this.company = company;
            this.isUserMessage = isUserMessage;
            this.text = text;
            sendingTime = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }
        public DateTime sendingTime { get; set; }
        public bool isUserMessage { get; set; }
        public User user { get; set; }
        public Company company { get; set; }
        public string text { get; set; }
    }

    public class CompanyNotification
    {
        public CompanyNotification()
        {
            createTime = DateTime.UtcNow;
        }
        public CompanyNotification(Company company, string title, string text)
        {
            this.company = company;
            this.title = title;
            this.text = text;
            createTime = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }
        public DateTime createTime { get; set; }
        public Company company { get; set; }
        public string title { get; set; }
        public string text { get; set; }
    }
}