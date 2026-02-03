using MediatR;
using AutoMapper;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.Forecasting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Forecasting.Queries.GetAllocations
{
    public record GetAllocationsQuery(int ForecastVersionId) : IRequest<List<ResourceAllocationDto>>;

    public class GetAllocationsQueryHandler : IRequestHandler<GetAllocationsQuery, List<ResourceAllocationDto>>
    {
        private readonly IForecastRepository _forecastRepository;
        private readonly IMapper _mapper;

        public GetAllocationsQueryHandler(IForecastRepository forecastRepository, IMapper mapper)
        {
            _forecastRepository = forecastRepository;
            _mapper = mapper;
        }

        public async Task<List<ResourceAllocationDto>> Handle(GetAllocationsQuery request, CancellationToken cancellationToken)
        {
            var allocations = await _forecastRepository.GetAllocationsByVersionAsync(request.ForecastVersionId);
            return _mapper.Map<List<ResourceAllocationDto>>(allocations);
        }

    }
}
