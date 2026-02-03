using MediatR;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.Roster;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Roster.Queries.ExportRoster
{
    public record ExportRosterQuery : IRequest<byte[]>;

    public class ExportRosterQueryHandler : IRequestHandler<ExportRosterQuery, byte[]>
    {
        private readonly IRosterRepository _rosterRepository;
        private readonly IExcelService _excelService;

        public ExportRosterQueryHandler(IRosterRepository rosterRepository, IExcelService excelService)
        {
            _rosterRepository = rosterRepository;
            _excelService = excelService;
        }

        public async Task<byte[]> Handle(ExportRosterQuery request, CancellationToken cancellationToken)
        {
            var roster = await _rosterRepository.GetAllAsync();
            // In a real scenario, we might map to a flatter DTO specifically for Excel
            return await _excelService.ExportToExcelAsync(roster, "Roster");
        }
    }
}
