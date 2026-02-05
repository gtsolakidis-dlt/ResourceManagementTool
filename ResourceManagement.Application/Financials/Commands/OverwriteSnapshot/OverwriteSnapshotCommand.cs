using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Domain.Services;

namespace ResourceManagement.Application.Financials.Commands.OverwriteSnapshot
{
    /// <summary>
    /// Command to manually override calculated values for the editable month.
    /// Only the Editable month can be overwritten.
    /// NSR and Margin are recalculated automatically after save.
    /// </summary>
    public record OverwriteSnapshotCommand : IRequest<bool>
    {
        public int ProjectId { get; init; }
        public int ForecastVersionId { get; init; }
        public DateTime Month { get; init; }
        public string OverriddenBy { get; init; } = string.Empty;

        // Overrideable values (nullable - only provided values will be updated)
        public decimal? OpeningBalance { get; init; }
        public decimal? CumulativeBillings { get; init; }
        public decimal? Wip { get; init; }
        public decimal? DirectExpenses { get; init; }
        public decimal? OperationalCost { get; init; }
        public decimal? Nsr { get; init; }
        public decimal? Margin { get; init; }
    }

    public class OverwriteSnapshotCommandValidator : AbstractValidator<OverwriteSnapshotCommand>
    {
        public OverwriteSnapshotCommandValidator()
        {
            RuleFor(x => x.ProjectId).GreaterThan(0);
            RuleFor(x => x.ForecastVersionId).GreaterThan(0);
            RuleFor(x => x.Month).NotEmpty();
            RuleFor(x => x.OverriddenBy).NotEmpty();
        }
    }

    public class OverwriteSnapshotCommandHandler : IRequestHandler<OverwriteSnapshotCommand, bool>
    {
        private readonly IProjectMonthlySnapshotRepository _snapshotRepository;
        private readonly ISnapshotRecalculationService _recalculationService;

        public OverwriteSnapshotCommandHandler(
            IProjectMonthlySnapshotRepository snapshotRepository,
            ISnapshotRecalculationService recalculationService)
        {
            _snapshotRepository = snapshotRepository;
            _recalculationService = recalculationService;
        }

        public async Task<bool> Handle(OverwriteSnapshotCommand request, CancellationToken cancellationToken)
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
                throw new InvalidOperationException("Only the Editable month can be overwritten.");
            }

            // Validate: NSR and Margin are calculated fields and cannot be overwritten directly
            if (request.Nsr.HasValue || request.Margin.HasValue)
            {
                throw new InvalidOperationException("NSR and Margin are calculated fields and cannot be manually overwritten.");
            }

            // Capture original values if this is the first override
            if (!snapshot.IsOverridden)
            {
                snapshot.OriginalOpeningBalance = snapshot.OpeningBalance;
                snapshot.OriginalCumulativeBillings = snapshot.CumulativeBillings;
                snapshot.OriginalWip = snapshot.Wip;
                snapshot.OriginalDirectExpenses = snapshot.DirectExpenses;
                snapshot.OriginalOperationalCost = snapshot.OperationalCost;
            }

            // Apply overrides (only non-null values for editable fields)
            if (request.OpeningBalance.HasValue) snapshot.OpeningBalance = request.OpeningBalance.Value;
            if (request.CumulativeBillings.HasValue) snapshot.CumulativeBillings = request.CumulativeBillings.Value;
            if (request.Wip.HasValue) snapshot.Wip = request.Wip.Value;
            if (request.DirectExpenses.HasValue) snapshot.DirectExpenses = request.DirectExpenses.Value;
            if (request.OperationalCost.HasValue) snapshot.OperationalCost = request.OperationalCost.Value;

            // Mark as overridden
            snapshot.IsOverridden = true;
            snapshot.OverriddenAt = DateTime.UtcNow;
            snapshot.OverriddenBy = request.OverriddenBy;
            snapshot.UpdatedAt = DateTime.UtcNow;

            await _snapshotRepository.UpdateAsync(snapshot);

            // Trigger propagation using Recalculation Service
            // This will recalculate NSR/Margin for this month AND propagate deltas to all future pending months
            await _recalculationService.RecalculateFromMonthAsync(
                request.ProjectId, 
                request.ForecastVersionId, 
                request.Month,
                false);

            return true;
        }
    }
}
