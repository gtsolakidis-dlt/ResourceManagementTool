using AutoMapper;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Contracts.Project;

namespace ResourceManagement.Application.Projects.Common
{
    public class ProjectMappingProfile : Profile
    {
        public ProjectMappingProfile()
        {
            CreateMap<Project, ProjectDto>();
        }
    }
}
