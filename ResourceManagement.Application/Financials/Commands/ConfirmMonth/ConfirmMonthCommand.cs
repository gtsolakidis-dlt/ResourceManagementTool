using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Application.Financials.Commands.ConfirmMonth
{
    /// <summary>
    /// Command to confirm the editable month, locking its values and promoting the next pending month to editable.
    /// After confirmation, OB is propagated to future pending months and NSR/Margin are recalculated.
    /// </summary>
    public record ConfirmMonthCommand : IRequest<bool>
    {
        public int ProjectId { get; init; }
        public int ForecastVersionId { get; init; }
        public DateTime Month { get; init; }
        public string ConfirmedBy { get; init; } = string.Empty;
    }

    public class ConfirmMonthCommandValidator : AbstractValidator<ConfirmMonthCommand>
    {
        public ConfirmMonthCommandValidator()
        {
            RuleFor(x => x.ProjectId).GreaterThan(0);
            RuleFor(x => x.ForecastVersionId).GreaterThan(0);
            RuleFor(x => x.Month).NotEmpty();
            RuleFor(x => x.ConfirmedBy).NotEmpty();
        }
    }

    public class ConfirmMonthCommandHandler : IRequestHandler<ConfirmMonthCommand, bool>
    {
        private readonly IProjectMonthlySnapshotRepository _snapshotRepository;

        public ConfirmMonthCommandHandler(IProjectMonthlySnapshotRepository snapshotRepository)
        {
            _snapshotRepository = snapshotRepository;
        }

        public async Task<bool> Handle(ConfirmMonthCommand request, CancellationToken cancellationToken)
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
                throw new InvalidOperationException("Only the Editable month can be confirmed.");
            }

            // Store the confirmed OB value for propagation
            decimal confirmedOpeningBalance = snapshot.OpeningBalance;

            // Confirm the month
            var confirmed = await _snapshotRepository.ConfirmMonthAsync(snapshot.Id, request.ConfirmedBy);

            if (confirmed)
            {
                // Promote the next Pending month to Editable
                await _snapshotRepository.PromoteNextPendingToEditableAsync(
                    request.ProjectId, request.ForecastVersionId);

                // Propagate OB and recalculate NSR/Margin for all future pending months
                await PropagateValuesToFutureMonthsAsync(
                    request.ProjectId,
                    request.ForecastVersionId,
                    request.Month,
                    confirmedOpeningBalance);
            }

            return confirmed;
        }

        /// <summary>
        /// Propagates confirmed values (OB) to subsequent months and recalculates NSR/Margin.
        /// </summary>
        private async Task PropagateValuesToFutureMonthsAsync(
            int projectId,
            int forecastVersionId,
            DateTime fromMonth,
            decimal openingBalance)
        {
            // Get all non-confirmed months after the current month
            var futureSnapshots = await _snapshotRepository.GetNonConfirmedFromMonthAsync(
                projectId, forecastVersionId, fromMonth.AddMonths(1));

            foreach (var futureSnapshot in futureSnapshots.OrderBy(s => s.Month))
            {
                // Only propagate to non-overridden snapshots
                if (!futureSnapshot.IsOverridden)
                {
                    futureSnapshot.OpeningBalance = openingBalance;
                    
                    // Recalculate NSR: WIP + CB - OB - DE
                    futureSnapshot.Nsr = futureSnapshot.Wip + futureSnapshot.CumulativeBillings 
                                       - futureSnapshot.OpeningBalance - futureSnapshot.DirectExpenses;
                    
                    // Recalculate Margin: (NSR - OC) / NSR
                    futureSnapshot.Margin = futureSnapshot.Nsr == 0 ? 0 
                                          : (futureSnapshot.Nsr - futureSnapshot.OperationalCost) / futureSnapshot.Nsr;
                    
                    futureSnapshot.UpdatedAt = DateTime.UtcNow;
                    await _snapshotRepository.UpdateAsync(futureSnapshot);
                }
            }
        }
    }
}
