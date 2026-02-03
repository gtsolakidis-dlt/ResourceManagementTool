using AutoMapper;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Contracts.GlobalRates;

namespace ResourceManagement.Application.GlobalRates.Common
{
    public class GlobalRateMappingProfile : Profile
    {
        public GlobalRateMappingProfile()
        {
            CreateMap<GlobalRate, GlobalRateDto>();
        }
    }
}
