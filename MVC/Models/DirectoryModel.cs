#nullable disable
namespace MVC.Models
{
    public class DirectoryModel
    {
        public int Id { get; set; }

        public string Name
        {
            get
            {
                return Path.Split('\\').Last();
            }
        }
        public string Path { get; set; }

        public List<FileListModel> Files { get; set; } = new List<FileListModel> { };

        public List<DirectoryModel> SubDirectories { get; set; } = new List<DirectoryModel>();

        public DateTime ModifyDate { get; set; }
    }
}
