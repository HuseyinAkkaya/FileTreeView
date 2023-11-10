using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Contexts;
using MVC.Entities;
using MVC.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MVC.api
{
    public class FileController : ControllerBase
    {
        private readonly Db _db;
        private readonly IMapper _mapper;

        public FileController(Db db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        [HttpPost]
        public string MoveFile(int fileId, int newDirectoryId)
        {
            try
            {
                var file = _db.Files.FirstOrDefault(x => x.Id == fileId);
                if (file != null)
                {
                    file!.DirectoryId = newDirectoryId;
                    _db.Entry(file).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _db.SaveChanges();
                }
                return "Successfull";
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public string MoveDirectory(int directoryId, int newParentId)
        {
            try
            {
                var directory = _db.Directories.FirstOrDefault(x => x.Id == directoryId);
                if (directory != null)
                {
                    directory!.ParentDirectoryId = newParentId;
                    _db.Entry(directory).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _db.SaveChanges();
                }
                return "Successfull";
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public string CopyFile(int fileId, int newDirectoryId)
        {
            try
            {
                var file = _db.Files.FirstOrDefault(x => x.Id == fileId);
                var directory = _db.Directories.Include("Files").FirstOrDefault(x => x.Id == newDirectoryId);
                if (file != null && directory != null)
                {
                    FileEntity newEntity = _mapper.Map<FileEntity, FileEntity>(file);
                    directory.Files.Add(newEntity);
                    _db.Entry(directory).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _db.SaveChanges();
                }
                return "Successfull";
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public string CopyDirectory(int directoryId, int newParentId)
        {
            try
            {
                var dirs = _db.Directories.Include(x => x.SubDirectories).Include(x => x.Files).ToList();
                var directory = dirs.FirstOrDefault(x => x.Id == directoryId);
                var parentDirectory = _db.Directories.Include("SubDirectories").FirstOrDefault(x => x.Id == newParentId);
                if (directory != null && parentDirectory != null)
                {
                    DirectoryEntity newEntity = _mapper.Map<DirectoryEntity, DirectoryEntity>(directory);
                    parentDirectory.SubDirectories.Add(newEntity);
                    _db.Entry(parentDirectory).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _db.SaveChanges();
                }
                return "Successfull";
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public string RemoveDirectory(int directoryId)
        {
            try
            {
                FileManager fm = new FileManager(_db);
                if (fm.RemoveDirectory(directoryId))
                {

                    return "Successfull";
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public string RemoveFile(int fileId)
        {
            try
            {
                FileManager fm = new FileManager(_db);
                if (fm.RemoveFile(fileId))
                {
                    return "Successfull";
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public string RenameFile(int fileId, string name)
        {
            try
            {
                FileManager fm = new FileManager(_db);
                if (fm.RenameFile(fileId, name))
                {
                    return "Successfull";
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public string RenameDirectory(int DirectoryId, string name)
        {
            try
            {
                FileManager fm = new FileManager(_db);
                if (fm.RenameDirectory(DirectoryId, name))
                {
                    return "Successfull";
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public string CreateDirectory(int DirectoryId, string name)
        {
            FileManager fm = new FileManager(_db);
            int? id = fm.CreateDirectory(DirectoryId, name);
            if (id.HasValue)
            {
                return id.Value.ToString();
            }
            else
            {
                throw new Exception();
            }
        }
        [HttpPost]
        public string CreateFile(int DirectoryId, string name)
        {
            FileManager fm = new FileManager(_db);
            int? id = fm.CreateFile(DirectoryId, name);
            if (id.HasValue)
            {
                return id.Value.ToString();
            }
            else
            {
                throw new Exception();
            }
        }

        [HttpPost]
        public string UploadFile(int directoryId, IFormFile fileToUpload)
        {
            if (fileToUpload != null && fileToUpload.Length > 0)
            {
                try
                {
                    string path = Path.Combine(Path.GetFullPath("wwwroot"), "Downloads");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string fname = Path.Combine(path, fileToUpload.FileName);
                    using var stream = System.IO.File.Create(fname);
                    fileToUpload.CopyTo(stream);
                    stream.Flush();
                    stream.Close();

                    FileManager fm = new FileManager(_db);
                    int? id = fm.CreateUploadedFile(directoryId, fname);
                    if (id.HasValue)
                    {
                        return id.Value.ToString();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error occurred. Error details:" + ex.Message);
                }
            }
            else
            {
                throw new Exception("Error occurred. Error details:");
            }
        }
        [HttpPost]
        public string SaveFile(int FileId, string Text)
        {
            try
            {
                var file = _db.Files.FirstOrDefault(x => x.Id == FileId);
                if (file != null)
                {
                    file.Content = Text;
                    _db.Entry(file).State = EntityState.Modified;
                    _db.SaveChanges();
                }
                return "Successfull";
            }
            catch (Exception)
            {
                throw;
            }
        }

    }


}
