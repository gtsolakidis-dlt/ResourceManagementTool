using MediatR;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Domain.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Roster.Commands.ImportRoster
{
    public record ImportRosterCommand(Stream FileStream) : IRequest<int>;

    public class ImportRosterCommandHandler : IRequestHandler<ImportRosterCommand, int>
    {
        private readonly IRosterRepository _rosterRepository;
        private readonly IExcelService _excelService;

        public ImportRosterCommandHandler(IRosterRepository rosterRepository, IExcelService excelService)
        {
            _rosterRepository = rosterRepository;
            _excelService = excelService;
        }

        public async Task<int> Handle(ImportRosterCommand request, CancellationToken cancellationToken)
        {
            var importedData = await _excelService.ImportFromExcelAsync<ResourceManagement.Domain.Entities.Roster>(request.FileStream);
            if (importedData == null || importedData.Count == 0) return 0;

            var existingMembers = await _rosterRepository.GetAllAsync();
            var existingSapCodes = existingMembers.ToDictionary(m => m.SapCode, m => m.Id);

            int count = 0;
            foreach (var member in importedData)
            {
                if (string.IsNullOrWhiteSpace(member.SapCode)) continue;

                if (existingSapCodes.TryGetValue(member.SapCode, out var existingId))
                {
                    member.Id = existingId;
                    member.UpdatedAt = System.DateTime.UtcNow;
                    await _rosterRepository.UpdateAsync(member);
                }
                else
                {
                    member.CreatedAt = System.DateTime.UtcNow;
                    member.UpdatedAt = System.DateTime.UtcNow;
                    await _rosterRepository.CreateAsync(member);
                }
                count++;
            }
            return count;
        }
    }
}
