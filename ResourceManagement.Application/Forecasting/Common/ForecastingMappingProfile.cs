using AutoMapper;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Contracts.Forecasting;

namespace ResourceManagement.Application.Forecasting.Common
{
    public class ForecastingMappingProfile : Profile
    {
        public ForecastingMappingProfile()
        {
            CreateMap<ResourceAllocation, ResourceAllocationDto>();
            CreateMap<ForecastVersion, ForecastVersionDto>();
        }
    }
}
