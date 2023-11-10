#nullable disable

using AppCoreLite.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Entities
{
    public class FileEntity
    {
        public int Id { get; set; }
        [Required]
        public string FileName { get; set; } 
        public string Extention { get; set; } 
        public FileTypes FileType { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; } 
        public DateTime ModifyDate { get; set; }
        public int DirectoryId { get; set; }
        public DirectoryEntity Directory { get; set; }

        [NotMapped]
        public string FullPath
        {
            get
            {
                if (Directory == null)
                {
                    return FileName;
                }
                else
                {
                    return Directory.Name + "\\" + FileName;
                }
            }
        }

    }

    internal class FileConfiguration : IEntityTypeConfiguration<FileEntity>
    {
        public void Configure(EntityTypeBuilder<FileEntity> builder)
        {
            builder.HasOne(e => e.Directory)
                .WithMany(r => r.Files)
                .HasForeignKey(e => e.DirectoryId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
