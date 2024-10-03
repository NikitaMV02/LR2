using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NLog;
using Dapper;
using System.Data.SQLite;  
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace DotNetStandardDemo
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UserDto
    {
        public string FullName { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=users.db");
        }
    }

    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Batteries.Init();  

            // 1. Використання Newtonsoft.Json
            var user = new User { Id = 1, Name = "John 2" };
            string json = JsonConvert.SerializeObject(user);
            Console.WriteLine($"Serialized JSON: {json}");

            // 2. Використання NLog для логування
            var config1 = new NLog.Config.LoggingConfiguration();
            var consoleTarget = new NLog.Targets.ConsoleTarget("console");

            config1.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget);

            NLog.LogManager.Configuration = config1;

            logger.Info("Програма стартувала");



            // 3. Використання Dapper для SQL-запиту
            using (var connection = new System.Data.SQLite.SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.Execute("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT)");
                connection.Execute("INSERT INTO Users (Name) VALUES (@Name)", new { Name = "John" });

                var users = connection.Query<User>("SELECT * FROM Users");
                foreach (var u in users)
                {
                    Console.WriteLine($"User: {u.Name}");
                }
            }

            // 4. Використання AutoMapper для мапінгу об'єктів
            var config = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDto>()
                                                         .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name)));
            var mapper = new Mapper(config);
            var userDto = mapper.Map<UserDto>(user);
            Console.WriteLine($"Mapped DTO: {userDto.FullName}");

            // 5. Використання Entity Framework Core для роботи з базою даних
            using (var db = new AppDbContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.Users.Add(new User { Name = "Denys" });
                db.SaveChanges();

                var savedUsers = db.Users.ToListAsync().Result;
                foreach (var u in savedUsers)
                {
                    Console.WriteLine($"EF Core User: {u.Name}");
                }
            }

            // Логування завершення
            logger.Info("Програма завершила роботу");
        }
    }
}
