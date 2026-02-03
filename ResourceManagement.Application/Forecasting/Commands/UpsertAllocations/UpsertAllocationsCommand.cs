using System;
using MediatR;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Application.Financials.Notifications;
using ResourceManagement.Contracts.Forecasting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Forecasting.Commands.UpsertAllocations
{
    public record UpsertAllocationsCommand(
        int ForecastVersionId,
        int RosterId,
        List<AllocationUpsertDto> Allocations) : IRequest<Unit>;

    public record AllocationUpsertDto(DateTime Month, decimal AllocatedDays);

    public class UpsertAllocationsCommandHandler : IRequestHandler<UpsertAllocationsCommand, Unit>
    {
        private readonly IForecastRepository _forecastRepository;
        private readonly IMediator _mediator;

        public UpsertAllocationsCommandHandler(
            IForecastRepository forecastRepository,
            IMediator mediator)
        {
            _forecastRepository = forecastRepository;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(UpsertAllocationsCommand request, CancellationToken cancellationToken)
        {
            // Get the forecast version to find the project ID
            var forecastVersion = await _forecastRepository.GetVersionByIdAsync(request.ForecastVersionId);
            if (forecastVersion == null)
            {
                throw new InvalidOperationException($"Forecast version {request.ForecastVersionId} not found.");
            }

            DateTime? earliestAffectedMonth = null;

            foreach (var a in request.Allocations)
            {
                await _forecastRepository.UpsertAllocationAsync(new ResourceAllocation
                {
                    ForecastVersionId = request.ForecastVersionId,
                    RosterId = request.RosterId,
                    Month = a.Month,
                    AllocatedDays = a.AllocatedDays
                });

                // Track the earliest affected month for notification
                if (!earliestAffectedMonth.HasValue || a.Month < earliestAffectedMonth)
                {
                    earliestAffectedMonth = a.Month;
                }
            }

            // Publish notification to trigger snapshot recalculation
            if (earliestAffectedMonth.HasValue)
            {
                await _mediator.Publish(new FinancialDataChangedNotification(
                    forecastVersion.ProjectId,
                    request.ForecastVersionId,
                    earliestAffectedMonth.Value,
                    "Allocation"
                ), cancellationToken);
            }

            return Unit.Value;
        }

    }
}
