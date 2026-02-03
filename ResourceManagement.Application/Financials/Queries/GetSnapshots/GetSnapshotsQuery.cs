using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.Financials;

namespace ResourceManagement.Application.Financials.Queries.GetSnapshots
{
    public record GetSnapshotsQuery(int ProjectId, int ForecastVersionId) : IRequest<List<ProjectMonthlySnapshotDto>>;

    public class GetSnapshotsQueryHandler : IRequestHandler<GetSnapshotsQuery, List<ProjectMonthlySnapshotDto>>
    {
        private readonly IProjectMonthlySnapshotRepository _snapshotRepository;

        public GetSnapshotsQueryHandler(IProjectMonthlySnapshotRepository snapshotRepository)
        {
            _snapshotRepository = snapshotRepository;
        }

        public async Task<List<ProjectMonthlySnapshotDto>> Handle(GetSnapshotsQuery request, CancellationToken cancellationToken)
        {
            var snapshots = await _snapshotRepository.GetByProjectAsync(request.ProjectId, request.ForecastVersionId);

            return snapshots.Select(s => new ProjectMonthlySnapshotDto
            {
                Id = s.Id,
                ProjectId = s.ProjectId,
                ForecastVersionId = s.ForecastVersionId,
                Month = s.Month,
                Status = s.Status.ToString(),
                OpeningBalance = s.OpeningBalance,
                CumulativeBillings = s.CumulativeBillings,
                Wip = s.Wip,
                DirectExpenses = s.DirectExpenses,
                OperationalCost = s.OperationalCost,
                MonthlyBillings = s.MonthlyBillings,
                MonthlyExpenses = s.MonthlyExpenses,
                CumulativeExpenses = s.CumulativeExpenses,
                Nsr = s.Nsr,
                Margin = s.Margin,
                
                OriginalOpeningBalance = s.OriginalOpeningBalance,
                OriginalCumulativeBillings = s.OriginalCumulativeBillings,
                OriginalWip = s.OriginalWip,
                OriginalDirectExpenses = s.OriginalDirectExpenses,
                OriginalOperationalCost = s.OriginalOperationalCost,

                IsOverridden = s.IsOverridden,
                OverriddenAt = s.OverriddenAt,
                OverriddenBy = s.OverriddenBy,
                ConfirmedAt = s.ConfirmedAt,
                ConfirmedBy = s.ConfirmedBy
            }).ToList();
        }
    }
}
