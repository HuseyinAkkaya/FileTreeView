using AppCoreLite.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using MVC.Contexts;
using MVC.Entities;
using MVC.Models;
using System.IO;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace MVC.Services
{
    public class FileManager
    {
        private readonly Db _db;

        public FileManager(Db db)
        {
            _db = db;
        }

        Dictionary<string, string> textFiles = new Dictionary<string, string>
        {
                { ".txt", "plaintext" },
                { ".json", "json" },
                { ".xml", "xml" },
                { ".htm", "html" },
                { ".html", "html" },
                { ".css", "css" },
                { ".js", "javascript" },
                { ".cs", "csharp" },
                { ".java", "java" },
                { ".sql", "sql" },
                { ".cshtml", "html" }
        };
        Dictionary<string, string> imageFiles = new Dictionary<string, string>()
        {
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" }
        };
        Dictionary<string, string> otherFiles = new Dictionary<string, string>()
        {
                { ".zip", "application/zip" },
                { ".7z", "application/zip" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" }
        };

        public string AddOrUpdateCodes(string FilePath, string ProjectName ,string ProjectPath, string[] AllowExtentions, string Description = "")
        {
            if (!Directory.Exists(FilePath))
            {
                return FilePath + " Not exists";
            }
            if (!Directory.Exists(ProjectPath))
            {
                return ProjectPath + " Not exists";
            }
            FilePath = FilePath.Trim().TrimEnd('\\');
            StringBuilder sb = new StringBuilder();
            try
            {
                var dirModel = GetDirectoryHiyerarchy(FilePath, AllowExtentions);
                if (_db.Directories.Any())
                {
                    _db.RemoveRange(_db.Files.ToList());
                    _db.RemoveRange(_db.Directories.ToList());
                    _db.Database.ExecuteSqlRaw("DBCC CHECKIDENT('Files', RESEED, 0)");
                    _db.Database.ExecuteSqlRaw("DBCC CHECKIDENT('Directories', RESEED, 0)");
                    _db.SaveChanges();

                }

                var directory = DirectoryFromModel(dirModel, FilePath, ProjectPath, ProjectName.Trim());
                _db.Directories.Add(directory);
                _db.SaveChanges();
                sb.AppendLine("Successfull");
            }
            catch (Exception exc)
            {
                sb.AppendLine(exc.ToString());
            }
            return sb.ToString();
        }

        public DirectoryModel GetDirectoryHiyerarchy(string FilePath, params string[] AllowExtentions)
        {
            DirectoryModel directoryModel = new DirectoryModel();
            directoryModel.Path = FilePath;
            directoryModel.ModifyDate = Directory.GetLastWriteTime(FilePath);
            List<string> localFiles = new List<string>();
            if (AllowExtentions.Any())
            {
                localFiles.AddRange(Directory.GetFiles(FilePath).ToList().Where(f => AllowExtentions.Any(e => f.ToLower().EndsWith(e.ToLower()))));
            }
            else
            {
                localFiles.AddRange(Directory.GetFiles(FilePath).ToList());
            }
            foreach (string file in localFiles)
            {
                var ext = Path.GetExtension(file);
                directoryModel.Files.Add(new FileModel()
                {
                    FileName = Path.GetFileName(file),
                    DirectoryName = Path.GetDirectoryName(file),
                    Extention = Path.GetExtension(file),
                    FullPath = Path.GetFullPath(file),
                    ModifyDate = File.GetLastWriteTime(file),
                    FileType =
                        textFiles.ContainsKey(ext.ToLower()) ? FileTypes.Text :
                        imageFiles.ContainsKey(ext.ToLower()) ? FileTypes.Image :
                        otherFiles.ContainsKey(ext.ToLower()) ? FileTypes.Other :
                        FileTypes.None,
                    ContentType =
                        textFiles.ContainsKey(ext.ToLower()) ? textFiles[ext.ToLower()] :
                        imageFiles.ContainsKey(ext.ToLower()) ? imageFiles[ext.ToLower()] :
                        otherFiles.ContainsKey(ext.ToLower()) ? otherFiles[ext.ToLower()] : null,
                });
            }
            if (Directory.GetDirectories(FilePath).Length > 0)
            {
                foreach (var dirs in Directory.GetDirectories(FilePath))
                {
                    directoryModel.SubDirectories.Add(GetDirectoryHiyerarchy(dirs, AllowExtentions));
                }
            }
            return directoryModel;
        }

        public DirectoryEntity DirectoryFromModel(DirectoryModel model, string FilePath ,string ProjectPath, string ProjectName)
        {
            DirectoryEntity directory = new DirectoryEntity()
            {
                Files = model.Files.Select(x =>
                {
                    var newFile = new FileEntity()
                    {
                        Extention = x.Extention,
                        FileName = x.FileName,
                        ModifyDate = x.ModifyDate,
                        FileType = x.FileType,
                        ContentType = x.ContentType,
                        Content = x.FileType == FileTypes.Text ? File.ReadAllText(x.FullPath, Encoding.GetEncoding(1254)) : null 
                    };
                    if (newFile.FileType == FileTypes.Text)
                    {
                        newFile.Content = File.ReadAllText(x.FullPath, Encoding.GetEncoding(1254)); 
                    }
                    else
                    {
                        string newname = Guid.NewGuid().ToString();
                        FileInfo fileInfo = new FileInfo(x.FullPath);
                        string newPath = Path.Combine(ProjectPath,"Uploads",newname );
                         
                        if (!Directory.Exists(newPath))
                        {
                            Directory.CreateDirectory(newPath);
                        }
                        newPath = Path.Combine(newPath, newFile.FileName); 
                        fileInfo.CopyTo(newPath);
                        newFile.Content = "/Uploads/" + newname + "/" + newFile.FileName;
                    }
                    return newFile;
                }).ToList(),
                SubDirectories = new List<DirectoryEntity>(),
                ModifyDate = model.ModifyDate,
                Name = model.Path.Replace(FilePath, ProjectName).Split('\\').Last(),

            };
            foreach (var item in model.SubDirectories)
            {
                directory.SubDirectories.Add(DirectoryFromModel(item, FilePath, ProjectPath, ProjectName));
            }
            return directory;
        }

        public bool RemoveDirectory(int id, bool SaveChanges = true)
        {
            try
            {
                var directoryEntity = _db.Directories.Include(x => x.SubDirectories).FirstOrDefault(x => x.Id == id);
                if (directoryEntity != null)
                {
                    if (directoryEntity.SubDirectories.Count > 0)
                    {
                        for (int i = 0; i < directoryEntity.SubDirectories.Count; i++)
                        {
                            RemoveDirectory(directoryEntity.SubDirectories[i].Id, false);
                        }
                    }
                    var files = _db.Files.Where(x => x.DirectoryId == directoryEntity.Id).ToList();
                    if (files.Count > 0)
                    {
                        _db.Files.RemoveRange(files);
                    }
                    _db.Directories.Remove(directoryEntity);
                    if (SaveChanges)
                    {
                        _db.SaveChanges();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
        public bool RemoveFile(int id)
        {
            try
            {
                var file = _db.Files.FirstOrDefault(x => x.Id == id);
                if (file != null)
                {
                    _db.Files.Remove(file);
                    _db.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool RenameFile(int fileId, string name)
        {
            try
            {
                var f = _db.Files.FirstOrDefault(x => x.Id == fileId);
                if (f != null)
                {
                    f.FileName = name.Trim();
                    _db.Entry(f).State = EntityState.Modified;
                    _db.SaveChanges();
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                return false;
            }
            return true;

        }

        public bool RenameDirectory(int directoryId, string name)
        {
            try
            {
                var d = _db.Directories.FirstOrDefault(x => x.Id == directoryId);
                if (d != null)
                {
                    d.Name = name.Trim();
                    _db.Entry(d).State = EntityState.Modified;
                    _db.SaveChanges();
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public int? CreateDirectory(int directoryId, string name)
        {
            try
            {
                var d = _db.Directories.Include(x => x.SubDirectories).FirstOrDefault(x => x.Id == directoryId);
                if (d != null)
                {
                    DirectoryEntity newdir = new DirectoryEntity()
                    {
                        ModifyDate = DateTime.Now,
                        Name = name,

                    };
                    d.SubDirectories.Add(newdir);
                    _db.SaveChanges();
                    return newdir.Id;
                }

            }
            catch (Exception)
            {
            }
            return null;
        }

        public int? CreateFile(int directoryId, string name)
        {
            try
            {
                var d = _db.Directories.Include(x => x.Files).FirstOrDefault(x => x.Id == directoryId);
                if (d != null)
                {
                    name = name.Trim();
                    name = name.Split('.').Length > 1 ? name : name + "txt";
                    FileEntity newFile = new FileEntity()
                    {
                        ModifyDate = DateTime.Now,
                        FileName = name,
                        Extention = "." + name.Split('.').Last(),
                        Content = ""
                    };
                    newFile.FileType =
                        textFiles.ContainsKey(newFile.Extention.ToLower()) ? FileTypes.Text :
                        imageFiles.ContainsKey(newFile.Extention.ToLower()) ? FileTypes.Image :
                        otherFiles.ContainsKey(newFile.Extention.ToLower()) ? FileTypes.Other :
                        FileTypes.None;
                    newFile.ContentType =
                        textFiles.ContainsKey(newFile.Extention.ToLower()) ? textFiles[newFile.Extention.ToLower()] :
                        imageFiles.ContainsKey(newFile.Extention.ToLower()) ? imageFiles[newFile.Extention.ToLower()] :
                        otherFiles.ContainsKey(newFile.Extention.ToLower()) ? otherFiles[newFile.Extention.ToLower()] : null;
                    d.Files.Add(newFile);
                    _db.SaveChanges();
                    return newFile.Id;
                }

            }
            catch (Exception)
            {
            }
            return null;
        }

        public int? CreateUploadedFile(int directoryId, string name)
        {
            try
            {
                var d = _db.Directories.Include(x => x.Files).FirstOrDefault(x => x.Id == directoryId);
                if (d != null)
                {
                    name = name.Trim();
                    name = Path.GetFileName(name).Split('.').Length > 1 ? name : name + ".file";
                    FileEntity newFile = new FileEntity()
                    {
                        ModifyDate = DateTime.Now,
                        FileName = Path.GetFileName(name),
                        Extention = "." + name.Split('.').Last(),
                        Content = ""
                    };
                    newFile.FileType =
                        textFiles.ContainsKey(newFile.Extention.ToLower()) ? FileTypes.Text :
                        imageFiles.ContainsKey(newFile.Extention.ToLower()) ? FileTypes.Image :
                        otherFiles.ContainsKey(newFile.Extention.ToLower()) ? FileTypes.Other :
                        FileTypes.None;
                    newFile.ContentType =
                        textFiles.ContainsKey(newFile.Extention.ToLower()) ? textFiles[newFile.Extention.ToLower()] :
                        imageFiles.ContainsKey(newFile.Extention.ToLower()) ? imageFiles[newFile.Extention.ToLower()] :
                        otherFiles.ContainsKey(newFile.Extention.ToLower()) ? otherFiles[newFile.Extention.ToLower()] : null;
                    if (newFile.FileType == FileTypes.Text)
                    {
                        newFile.Content = File.ReadAllText(name, Encoding.GetEncoding(1254));
                        File.Delete(name);
                    }
                    else
                    {
                        string newname = Guid.NewGuid().ToString();
                        FileInfo fileInfo = new FileInfo(name); 
                        string newPath = Path.Combine(fileInfo.Directory!.FullName, "Downloads", newname);
                        if (!Directory.Exists(newPath))
                        {
                            Directory.CreateDirectory(newPath);
                        }
                        newPath = Path.Combine(newPath, newFile.FileName);
                        fileInfo.MoveTo(newPath);
                        newFile.Content = "/Downloads/" + newname+  "/" + newFile.FileName;
                    }
                    d.Files.Add(newFile);
                    _db.SaveChanges();
                    return newFile.Id;
                } 
            }
            catch (Exception)
            {
            }
            return null;
        }

        [Obsolete("GetAllFiles is deprecated, please use GetDirectoryHiyerarchy instead.")]
        public List<FileListModel> GetAllFiles(string FilePath, params string[] Extentions)
        {
            List<FileListModel> Files = new List<FileListModel>();
            List<string> localFiles = new List<string>();
            if (Extentions.Any())
            {
                localFiles.AddRange(Directory.GetFiles(FilePath).ToList().Where(f => Extentions.Any(e => f.ToLower().EndsWith(e.ToLower()))));
            }
            else
            {
                localFiles.AddRange(Directory.GetFiles(FilePath).ToList());
            }
            foreach (string file in localFiles)
            {
                var ext = Path.GetExtension(file);
                Files.Add(new FileModel()
                {
                    FileName = Path.GetFileName(file),
                    DirectoryName = Path.GetDirectoryName(file),
                    Extention = Path.GetExtension(file),
                    FullPath = Path.GetFullPath(file),
                    ModifyDate = File.GetLastWriteTime(file),
                    FileType =
                        textFiles.ContainsKey(ext.ToLower()) ? FileTypes.Text :
                        imageFiles.ContainsKey(ext.ToLower()) ? FileTypes.Image :
                        otherFiles.ContainsKey(ext.ToLower()) ? FileTypes.Other :
                        FileTypes.None,
                    ContentType =
                        textFiles.ContainsKey(ext.ToLower()) ? textFiles[ext.ToLower()] :
                        imageFiles.ContainsKey(ext.ToLower()) ? imageFiles[ext.ToLower()] :
                        otherFiles.ContainsKey(ext.ToLower()) ? otherFiles[ext.ToLower()] : null,
                });
            }
            if (Directory.GetDirectories(FilePath).Length > 0)
            {
                foreach (var dirs in Directory.GetDirectories(FilePath))
                {
                    Files.AddRange(GetAllFiles(dirs, Extentions));
                }
            }
            return Files;
        }

    }
}
