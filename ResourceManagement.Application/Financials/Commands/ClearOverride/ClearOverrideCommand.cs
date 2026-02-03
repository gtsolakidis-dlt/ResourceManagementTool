using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Domain.Services;

namespace ResourceManagement.Application.Financials.Commands.ClearOverride
{
    /// <summary>
    /// Command to clear manual overrides for the editable month and restore calculated values.
    /// </summary>
    public record ClearOverrideCommand : IRequest<bool>
    {
        public int ProjectId { get; init; }
        public int ForecastVersionId { get; init; }
        public DateTime Month { get; init; }
    }

    public class ClearOverrideCommandValidator : AbstractValidator<ClearOverrideCommand>
    {
        public ClearOverrideCommandValidator()
        {
            RuleFor(x => x.ProjectId).GreaterThan(0);
            RuleFor(x => x.ForecastVersionId).GreaterThan(0);
            RuleFor(x => x.Month).NotEmpty();
        }
    }

    public class ClearOverrideCommandHandler : IRequestHandler<ClearOverrideCommand, bool>
    {
        private readonly IProjectMonthlySnapshotRepository _snapshotRepository;
        private readonly ISnapshotRecalculationService _recalculationService;

        public ClearOverrideCommandHandler(
            IProjectMonthlySnapshotRepository snapshotRepository,
            ISnapshotRecalculationService recalculationService)
        {
            _snapshotRepository = snapshotRepository;
            _recalculationService = recalculationService;
        }

        public async Task<bool> Handle(ClearOverrideCommand request, CancellationToken cancellationToken)
        {
            // Get the snapshot for the specified month
            var snapshot = await _snapshotRepository.GetByMonthAsync(
                request.ProjectId, request.ForecastVersionId, request.Month);

            if (snapshot == null)
            {
                throw new InvalidOperationException($"Snapshot not found for month {request.Month:yyyy-MM}.");
            }

            if (snapshot.Status != SnapshotStatus.Editable)
            {
                throw new InvalidOperationException("Only the Editable month can have overrides cleared.");
            }

            // Clear the override flag
            snapshot.IsOverridden = false;
            snapshot.OverriddenAt = null;
            snapshot.OverriddenBy = null;
            snapshot.UpdatedAt = DateTime.UtcNow;

            await _snapshotRepository.UpdateAsync(snapshot);

            // Trigger recalculation to restore calculated values
            await _recalculationService.RecalculateFromMonthAsync(
                request.ProjectId, request.ForecastVersionId, request.Month);

            return true;
        }
    }
}
