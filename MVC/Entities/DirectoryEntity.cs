#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace MVC.Entities
{
    //public class DirectoryEntity
    //{
    //    public int Id { get; set; }
    //    public string Path { get; set; }
    //    public DateTime ModifyDate { get; set; }
    //    public List<FileEntity> Files { get; set; }
    //    public int? ParentDirectoryId { get; set; } = null;
    //    public virtual DirectoryEntity ParentDirectory { get; set; }
    //    public virtual List<DirectoryEntity> SubDirectories { get; set; }
    //}

    //internal class DirectoryEntityConfiguration : IEntityTypeConfiguration<DirectoryEntity>
    //{
    //    public void Configure(EntityTypeBuilder<DirectoryEntity> builder)
    //    {
    //        builder.HasOne(e => e.ParentDirectory)
    //           .WithMany(e => e.SubDirectories)
    //        .OnDelete(DeleteBehavior.NoAction); 
    //    }
    //}

    public class DirectoryEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime ModifyDate { get; set; }
        public List<FileEntity> Files { get; set; }
        public int? ParentDirectoryId { get; set; }
        public virtual DirectoryEntity ParentDirectory { get; set; }
        public virtual List<DirectoryEntity> SubDirectories { get; set; }

        [NotMapped]
        public string Path
        {
            get
            {
                if (ParentDirectory == null)
                {
                    return Name;
                }
                else
                {
                    return ParentDirectory.Name + "\\" + Name;
                }

            }
        }
    }

    internal class DirectoryEntityConfiguration : IEntityTypeConfiguration<DirectoryEntity>
    {
        public void Configure(EntityTypeBuilder<DirectoryEntity> builder)
        {
            builder.HasOne(e => e.ParentDirectory)
               .WithMany(e => e.SubDirectories)
            .OnDelete(DeleteBehavior.NoAction);

        }
    }

}
