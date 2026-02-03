using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ResourceManagement.Domain.Services;

namespace ResourceManagement.Application.Financials.Notifications
{
    /// <summary>
    /// Notification published when financial data (Billing, Expense, or Allocation) changes.
    /// Triggers recalculation of non-Confirmed monthly snapshots.
    /// </summary>
    public record FinancialDataChangedNotification(
        int ProjectId,
        int ForecastVersionId,
        DateTime AffectedMonth,
        string ChangeType  // "Billing", "Expense", "Allocation"
    ) : INotification;

    /// <summary>
    /// Handler that recalculates snapshots when financial data changes.
    /// </summary>
    public class FinancialDataChangedNotificationHandler : INotificationHandler<FinancialDataChangedNotification>
    {
        private readonly ISnapshotRecalculationService _recalculationService;

        public FinancialDataChangedNotificationHandler(ISnapshotRecalculationService recalculationService)
        {
            _recalculationService = recalculationService;
        }

        public async Task Handle(FinancialDataChangedNotification notification, CancellationToken cancellationToken)
        {
            // Recalculate all non-Confirmed snapshots from the affected month onwards
            await _recalculationService.RecalculateFromMonthAsync(
                notification.ProjectId,
                notification.ForecastVersionId,
                notification.AffectedMonth);
        }
    }
}
