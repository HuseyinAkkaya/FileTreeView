using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Contexts;
using MVC.Entities;
using MVC.Models;
using MVC.Services;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace MVC.Controllers
{
    public class FileController : Controller
    {
        private readonly Db _db;
        private readonly IMapper _mapper;

        public FileController(Db db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            ViewBag.isAdmin = true;
            var dirs = _db.Directories.Include(x => x.SubDirectories).Include(x => x.Files).ToList();
            if (!dirs.Any())
            {
                return Content("Directory list is empty. Get Seed Method!");
            }
            var dir = dirs.FirstOrDefault(x => x.Name == "Contents");
            var res = _mapper.Map<DirectoryModel>(dir);
            return View(res);
        }
        public JsonResult GetContent(int Id)
        {
            var file = _db.Files.FirstOrDefault(x => x.Id == Id);
            if (file != null)
            {
                var dir = _db.Directories.Include(x => x.ParentDirectory).ToList().FirstOrDefault(x => x.Id == file.DirectoryId);
                var res = _mapper.Map<FileEntity, FileModel>(file!);
                res.FullPath = dir!.Path + "\\" + res.FileName;
                return Json(res);

            }
            return Json(null);

        }
        public JsonResult Search(string Text)
        {
            var files = _db.Files.Include(x => x.Directory).ToList();
            files = files.Where(x => x.Content.ToLower().Contains(Text.ToLower()) || x.FileName.Contains(Text.ToLower())).ToList();
            if (files != null)
            {
                var res = _mapper.Map<List<FileEntity>, List<FileListModel>>(files!);
                return Json(res);
            }
            return Json(null);

        }

        public ContentResult Seed()
        {
            StringBuilder sb = new StringBuilder();
            var res = (new FileManager(_db)).AddOrUpdateCodes("D:\\İndirilenler\\Contents", "Contents", Path.GetFullPath("wwwroot"), new string[] { });

            return Content(res);
        }
    }
}
