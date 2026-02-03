using AutoMapper;
using RosterEntity = ResourceManagement.Domain.Entities.Roster;
using ResourceManagement.Contracts.Roster;

namespace ResourceManagement.Application.Roster.Common
{
    public class RosterMappingProfile : Profile
    {
        public RosterMappingProfile()
        {
            CreateMap<RosterEntity, RosterDto>();
        }
    }
}

