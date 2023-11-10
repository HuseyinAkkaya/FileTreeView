using AutoMapper;
using MVC.Entities;
using MVC.Models;

namespace MVC.Configs
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Add as many of these lines as you need to map your objects 
            CreateMap<FileEntity, FileListModel>()
                .ReverseMap();
            CreateMap<FileEntity, FileModel>()
                .ReverseMap();
            CreateMap<DirectoryEntity, DirectoryModel>().ReverseMap();
            CreateMap<DirectoryEntity, DirectoryEntity>().AfterMap((s, d) => { d.ParentDirectoryId = 0; d.Id = 0; });
            CreateMap<FileEntity, FileEntity>().AfterMap((s, d) => { d.DirectoryId = 0; d.Id = 0; });
        }
    }
}
