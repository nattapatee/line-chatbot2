using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace centralloggerbot.Models
{
    public class Users
    {
        [Key]
        [JsonIgnore]
        public int Id { set; get; }
        public string LineId { set; get; }
    }

    public class DbCreateContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbCreateContext(DbContextOptions<DbCreateContext> options) : base(options) { }

    }
}