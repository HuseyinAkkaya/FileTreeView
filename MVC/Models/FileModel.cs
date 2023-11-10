#nullable disable
using AppCoreLite.Enums;
namespace MVC.Models
{
    public class FileListModel
    {
        public int Id { get; set; }

        public int DirectoryId { get; set; }

        public string FileName { get; set; }

        public string Extention { get; set; }

        public string DirectoryName { get; set; }

        public string FullPath { get; set; }
         
        public DateTime ModifyDate { get; set; }

        public FileTypes FileType { get; set; }

        public string ContentType { get; set; }


    }

    public class FileModel : FileListModel
    {
        public string Content { get; set; }
    }

}

