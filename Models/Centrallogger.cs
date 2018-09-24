using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CentralLogger
{

    public class LogInfo
    {
        [Key]
        [JsonIgnore]
        public int Id { set; get; }
        public LogLevel LogLevel { set; get; }
        public string Message { set; get; }
        public DateTime DateTime { set; get; } = DateTime.Now;
        public string Application { set; get; }
        public string Ip { set; get; }
        public string Category { set; get; }
    }
    public class Emails
    {
        [Key]
        [JsonIgnore]
        public int Id { set; get; }
        public string Application { set; get; }
        public string Email_1 { set; get; }
        public string Email_2 { set; get; }
        public string Email_3 { set; get; }
        public Boolean Enable { set; get; }
    }

    public class Users
    {
        [Key]
        public int Id { set; get; }
        public string User { set; get; }
        public string Password { set; get; }
    }

    public class Line
    {
        [Key]
        [JsonIgnore]
        public int Id { set; get; }
        public string LineId { set; get; }
        public string Application { set; get; }
    }
    public class CountLogs
    {
        public IEnumerable<int> dataInfos { set; get; }
        public IEnumerable<int> dataErrors { set; get; }
        public IEnumerable<int> dataDebugs { set; get; }
        public IEnumerable<int> dataTraces { set; get; }
        public IEnumerable<int> dataWarnings { set; get; }
        public IEnumerable<int> dataCriticals { set; get; }
    }

    public enum LogLevel
    {
        Trace, Debug, Information, Warning, Error, Critical
    }

    public class CentralLoggerContext : DbContext
    {
        public DbSet<LogInfo> LogInfos { get; set; }
        public DbSet<Emails> Emails { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Line> Line { get; set; }
        public CentralLoggerContext(DbContextOptions<CentralLoggerContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>()
                .HasAlternateKey(x => x.User)
                .HasName("AlternateKey_User");
        }
    }
}