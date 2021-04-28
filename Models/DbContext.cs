using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.PostgreSQL
{
    public class KindContext : DbContext
    {

        public readonly ILoggerFactory MyLoggerFactory;

        public KindContext()
        {
            MyLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLoggerFactory(MyLoggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .HasIndex(p => new { p.Email })
                .IsUnique(true);

            modelBuilder.Entity<Company>()
                .HasIndex(p => new { p.PlayerId })
                .IsUnique(true);

            modelBuilder.Entity<Company>()
               .HasIndex(p => new { p.INN, p.Address })
               .IsUnique();

            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(r => r.TimeZone)
                .HasDefaultValue("Asia/Novosibirsk");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(r => r.TimeOpen)
                .HasDefaultValue("2021-04-16 00:00:00");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(r => r.TimeClose)
                .HasDefaultValue("2021-04-16 23:59:59");
            });
        }

        public KindContext(DbContextOptions<KindContext> options) : base(options) { }
        public DbSet<Company> Company { get; set; }
        public DbSet<Offer> Offer { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<EmailCode> EmailCode { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<CompanyNotification> CompanyNotification { get; set; }
        public DbSet<LikedOffer> LikedOffer { get; set; }
        public DbSet<FavoriteCompany> FavoriteCompany { get; set; }
        public DbSet<Stories> Stories { get; set; }
        public DbSet<FileData> Files { get; set; }
    }

    public class FileData
    {
        [Key]
        public int Id { get; set; }
        public byte[] bytes { get; set; }
    }

    public class ProductCategory
    {
        public ProductCategory() { }
        public ProductCategory(ProductCategoryRequest request)
        {
            Name = request.name;
            AgeLimit = request.ageLimit;
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int AgeLimit { get; set; } = 0;
        public int? ImageId { get; set; }
        public virtual FileData Image { get; set; }
    }

    public class Company
    {
        public Company() { }
        public Company(CompanyRequest request, ProductCategory productCategory, string tz)
        {
            Name = request.name;
            NameOfficial = request.nameOfficial;
            Representative = request.representative;
            Email = request.email;
            INN = request.inn;
            Password = request.password;
            Address = request.address;
            TimeOpen = request.timeOpen;
            TimeClose = request.timeClose;
            ProductCategory = productCategory;
            EmailConfirmed = false;
            PlayerId = request.playerId;
            Phone = request.phone;
            Latitude = request.Latitude;
            Longitude = request.Longitude;
            TimeZone = tz;
        }

        [Key]
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string TimeZone { get; set; } = "Asia/Novosibirsk";
        public string NameOfficial { get; set; }
        public string Name { get; set; }
        public string Representative { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string INN { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public DateTime TimeOpen { get; set; }
        public DateTime TimeClose { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PlayerId { get; set; }
        public ProductCategory ProductCategory { get; set; }
        public int? ImageId { get; set; }
        public virtual FileData Image { get; set; }
    }

    public class Offer
    {
        public Offer() { }
        public Offer(OfferRequest request, Company company)
        {
            Text = request.text;
            DateStart = request.dateStart;
            DateEnd = request.dateEnd;
            TimeStart = (DateTime)(request.timeStart == null ? company.TimeOpen : request.timeStart);
            TimeEnd = (DateTime)(request.timeEnd == null ? company.TimeClose : request.timeEnd);
            Percentage = request.percentage;
            Company = company;
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
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public Company Company { get; set; }
        public int? ImageId { get; set; }
        public virtual FileData Image { get; set; }
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
            PlayerId = user.playerId;
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool isMan { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime BirthYear { get; set; }
        public string PlayerId { get; set; }
        public int? ImageId { get; set; }
        public virtual FileData Image { get; set; }
    }

    public class FavoriteCompany
    {
        public FavoriteCompany() { }

        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }

    public class LikedOffer
    {
        public LikedOffer() { }

        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int OfferId { get; set; }
        public virtual Offer Offer { get; set; }
    }

    public class Stories
    {
        public Stories() { }

        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int OfferId { get; set; }
        public virtual Offer Offer { get; set; }
    }

    public class EmailCode
    {
        public EmailCode()
        {
            createdDateTime = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }
        public string code { get; set; }
        public string email { get; set; }
        public DateTime createdDateTime { get; set; }
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