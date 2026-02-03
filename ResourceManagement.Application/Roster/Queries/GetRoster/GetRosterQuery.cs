using MediatR;
using AutoMapper;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.Roster;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Roster.Queries.GetRoster
{
    public record GetRosterQuery(int Id) : IRequest<RosterDto>;

    public class GetRosterQueryHandler : IRequestHandler<GetRosterQuery, RosterDto>
    {
        private readonly IRosterRepository _rosterRepository;
        private readonly IMapper _mapper;

        public GetRosterQueryHandler(IRosterRepository rosterRepository, IMapper mapper)
        {
            _rosterRepository = rosterRepository;
            _mapper = mapper;
        }

        public async Task<RosterDto> Handle(GetRosterQuery request, CancellationToken cancellationToken)
        {
            var roster = await _rosterRepository.GetByIdAsync(request.Id);
            if (roster == null)
            {
                throw new KeyNotFoundException($"Roster member with ID {request.Id} not found.");
            }

            return _mapper.Map<RosterDto>(roster);
        }
    }
}
