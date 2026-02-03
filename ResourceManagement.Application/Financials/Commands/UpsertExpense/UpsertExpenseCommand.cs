using System;
using System.Linq;
using MediatR;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Application.Financials.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Financials.Commands.UpsertExpense
{
    public record UpsertExpenseCommand(int ProjectId, DateTime Month, decimal Amount, int? ForecastVersionId = null) : IRequest<Unit>;

    public class UpsertExpenseCommandHandler : IRequestHandler<UpsertExpenseCommand, Unit>
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IForecastRepository _forecastRepository;
        private readonly IMediator _mediator;

        public UpsertExpenseCommandHandler(
            IExpenseRepository expenseRepository,
            IForecastRepository forecastRepository,
            IMediator mediator)
        {
            _expenseRepository = expenseRepository;
            _forecastRepository = forecastRepository;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(UpsertExpenseCommand request, CancellationToken cancellationToken)
        {
            await _expenseRepository.UpsertAsync(new Expense
            {
                ProjectId = request.ProjectId,
                Month = request.Month,
                Amount = request.Amount
            });

            // Publish notification to trigger snapshot recalculation
            var forecastVersionId = request.ForecastVersionId;
            if (!forecastVersionId.HasValue)
            {
                // Get the latest forecast version for this project
                var versions = await _forecastRepository.GetByProjectAsync(request.ProjectId);
                forecastVersionId = versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault()?.Id;
            }

            if (forecastVersionId.HasValue)
            {
                await _mediator.Publish(new FinancialDataChangedNotification(
                    request.ProjectId,
                    forecastVersionId.Value,
                    request.Month,
                    "Expense"
                ), cancellationToken);
            }

            return Unit.Value;
        }
    }
}
