using System;
using System.Linq;
using MediatR;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Application.Financials.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Financials.Commands.UpsertBilling
{
    public record UpsertBillingCommand(int ProjectId, DateTime Month, decimal Amount, int? ForecastVersionId = null) : IRequest<Unit>;

    public class UpsertBillingCommandHandler : IRequestHandler<UpsertBillingCommand, Unit>
    {
        private readonly IBillingRepository _billingRepository;
        private readonly IForecastRepository _forecastRepository;
        private readonly IMediator _mediator;

        public UpsertBillingCommandHandler(
            IBillingRepository billingRepository,
            IForecastRepository forecastRepository,
            IMediator mediator)
        {
            _billingRepository = billingRepository;
            _forecastRepository = forecastRepository;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(UpsertBillingCommand request, CancellationToken cancellationToken)
        {
            await _billingRepository.UpsertAsync(new Billing
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
                    "Billing"
                ), cancellationToken);
            }

            return Unit.Value;
        }
    }
}
