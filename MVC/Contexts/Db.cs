using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MVC.Entities;
using MVC.Models;

namespace MVC.Contexts
{
    public class Db : DbContext
    {
        public virtual DbSet<FileEntity> Files { get; set; }
        public virtual DbSet<DirectoryEntity> Directories { get; set; } 

        public Db(DbContextOptions options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DirectoryEntityConfiguration());
            modelBuilder.ApplyConfiguration(new FileConfiguration()); 
        }  
    }

}
