using MediatR;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Domain.Entities;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace ResourceManagement.Application.Forecasting.Commands.CloneVersion
{
    public record CloneVersionCommand(int ProjectId, int SourceVersionId) : IRequest<int>;

    public class CloneVersionCommandHandler : IRequestHandler<CloneVersionCommand, int>
    {
        private readonly IForecastRepository _forecastRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditRepository _auditRepository;

        public CloneVersionCommandHandler(
            IForecastRepository forecastRepository, 
            IUnitOfWork unitOfWork,
            IAuditRepository auditRepository)
        {
            _forecastRepository = forecastRepository;
            _unitOfWork = unitOfWork;
            _auditRepository = auditRepository;
        }

        public async Task<int> Handle(CloneVersionCommand request, CancellationToken cancellationToken)
        {
                var versions = await _forecastRepository.GetByProjectAsync(request.ProjectId);
                var nextNumber = (versions.Any() ? versions.Max(v => v.VersionNumber) : 0) + 1;

                var newVersion = new ForecastVersion
                {
                    ProjectId = request.ProjectId,
                    VersionNumber = nextNumber
                };

                var newVersionId = await _forecastRepository.CreateVersionAsync(newVersion);

                // Clone allocations
                var sourceAllocations = await _forecastRepository.GetAllocationsByVersionAsync(request.SourceVersionId);
                foreach (var alloc in sourceAllocations)
                {
                    await _forecastRepository.UpsertAllocationAsync(new ResourceAllocation
                    {
                        ForecastVersionId = newVersionId,
                        RosterId = alloc.RosterId,
                        Month = alloc.Month,
                        AllocatedDays = alloc.AllocatedDays
                    });
                }

                await _auditRepository.LogAsync(
                    "ForecastVersion", 
                    newVersionId.ToString(), 
                    "Clone", 
                    null, 
                    $"Cloned from VersionId {request.SourceVersionId}", 
                    "System");

                // _unitOfWork.Commit(); // Removed transaction commit
                return newVersionId;
        }

    }
}
