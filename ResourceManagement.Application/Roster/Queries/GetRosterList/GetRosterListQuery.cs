using MediatR;
using AutoMapper;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.Roster;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Roster.Queries.GetRosterList
{
    public record GetRosterListQuery : IRequest<List<RosterDto>>
    {
        public string? SearchTerm { get; init; }
        public string? FunctionBusinessUnit { get; init; }
        public string? Level { get; init; }
        public string? CostCenterCode { get; init; }
    }

    public class GetRosterListQueryHandler : IRequestHandler<GetRosterListQuery, List<RosterDto>>
    {
        private readonly IRosterRepository _rosterRepository;
        private readonly IForecastRepository _forecastRepository;
        private readonly IGlobalRateRepository _globalRateRepository;
        private readonly IMapper _mapper;

        public GetRosterListQueryHandler(
            IRosterRepository rosterRepository,
            IForecastRepository forecastRepository,
            IGlobalRateRepository globalRateRepository,
            IMapper mapper)
        {
            _rosterRepository = rosterRepository;
            _forecastRepository = forecastRepository;
            _globalRateRepository = globalRateRepository;
            _mapper = mapper;
        }

        public async Task<List<RosterDto>> Handle(GetRosterListQuery request, CancellationToken cancellationToken)
        {
            var rosterMembers = await _rosterRepository.SearchAsync(
                request.SearchTerm, 
                request.FunctionBusinessUnit, 
                request.Level, 
                request.CostCenterCode);

            var dtos = _mapper.Map<List<RosterDto>>(rosterMembers);

            // Fetch data for calculations
            var allocations = await _forecastRepository.GetAllLatestAllocationsAsync();
            var rates = await _globalRateRepository.GetAllAsync();

            // Calculate Projected Revenue
            foreach (var dto in dtos)
            {
                var memberAllocations = allocations.Where(a => a.RosterId == dto.Id);
                var rate = rates.FirstOrDefault(r => r.Level == dto.Level);
                var nominalRate = rate?.NominalRate ?? 0;
                var totalDays = memberAllocations.Sum(a => a.AllocatedDays);
                
                dto.ProjectedRevenue = totalDays * nominalRate;
            }

            return dtos;
        }
    }
}
