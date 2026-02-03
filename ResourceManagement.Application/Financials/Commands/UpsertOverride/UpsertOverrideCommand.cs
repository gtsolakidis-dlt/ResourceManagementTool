using MediatR;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Financials.Commands.UpsertOverride
{
    public record UpsertOverrideCommand : IRequest<int>
    {
        public int ProjectId { get; init; }
        public DateTime Month { get; init; }
        public decimal? OpeningBalance { get; init; }
        public decimal? Billings { get; init; }
        public decimal? Wip { get; init; }
        public decimal? Expenses { get; init; }
        public decimal? Cost { get; init; }
        public decimal? Nsr { get; init; }
        public decimal? Margin { get; init; }
    }

    public class UpsertOverrideCommandHandler : IRequestHandler<UpsertOverrideCommand, int>
    {
        private readonly IOverrideRepository _overrideRepository;

        public UpsertOverrideCommandHandler(IOverrideRepository overrideRepository)
        {
            _overrideRepository = overrideRepository;
        }

        public async Task<int> Handle(UpsertOverrideCommand request, CancellationToken cancellationToken)
        {
            var existing = await _overrideRepository.GetByMonthAsync(request.ProjectId, request.Month);
            
            if (existing != null && existing.Confirmed)
            {
                throw new Exception("Cannot update a confirmed override.");
            }

            // FR-6.1: Sequential Eligibility - Only the earliest unconfirmed month can be overridden.
            var allOverrides = await _overrideRepository.GetByProjectAsync(request.ProjectId);
            var confirmedMonths = allOverrides.Where(o => o.Confirmed).OrderBy(o => o.Month).ToList();
            
            if (confirmedMonths.Any())
            {
                var nextEligibleMonth = confirmedMonths.Last().Month.AddMonths(1);
                if (request.Month > nextEligibleMonth)
                {
                    throw new Exception($"Months must be overridden sequentially. The next eligible month is {nextEligibleMonth:MMMM yyyy}.");
                }
            }
            // Note: In a full implementation, we'd also check if request.Month < ProjectStartDate if no overrides exist.

            var @override = new Override
            {
                ProjectId = request.ProjectId,
                Month = request.Month,
                OpeningBalance = request.OpeningBalance,
                Billings = request.Billings,
                Wip = request.Wip,
                Expenses = request.Expenses,
                Cost = request.Cost,
                Nsr = request.Nsr,
                Margin = request.Margin,
                Confirmed = false
            };

            if (existing != null)
            {
                @override.Id = existing.Id;
                await _overrideRepository.UpdateAsync(@override);
                return existing.Id;
            }
            else
            {
                return await _overrideRepository.CreateAsync(@override);
            }
        }

    }
}
