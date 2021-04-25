using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Common.Mapping;
using TaskManager.Data.Models;

namespace TaskManager.Services.Models
{
    public class UserServiceModel : IMapFrom<Employee>, IHaveCustomMapping
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string TelephoneNumber { get; set; }

        public string MobileNumber { get; set; }

        public int? JobTitleId { get; set; }

        public string JobTitleName { get; set; }

        public int? SectorId { get; set; }

        public string SectorName { get; set; }

        public int? DepartmentId { get; set; }

        public string DepartmentName { get; set; }

        public int? DirectorateId { get; set; }

        public string DirectorateName { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public string DaeuAccaunt { get; set; }

        public bool isDeleted { get; set; }

        public bool isActive { get; set; }

        public bool Notify { get; set; }

        public void ConfigureMapping(Profile profile)
        {
            profile.CreateMap<Employee, UserServiceModel>()
                   .ForMember(u => u.DirectorateName, cfg => cfg.MapFrom(r => r.Directorate.DirectorateName))
                   .ForMember(u => u.DepartmentName, cfg => cfg.MapFrom(r => r.Department.DepartmentName))
                   .ForMember(u => u.SectorName, cfg => cfg.MapFrom(r => r.Sector.SectorName))
                   .ForMember(u => u.JobTitleName, cfg => cfg.MapFrom(r => r.JobTitle.TitleName))
                   .ForMember(u => u.RoleName, cfg => cfg.MapFrom(r => r.Role.Name));
        }
    }
}
