using MediatR;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.Financials;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Financials.Queries.GetFinancials
{
    public record GetProjectFinancialsQuery(int ProjectId, int ForecastVersionId) : IRequest<List<MonthlyFinancialDto>>;

    public class GetProjectFinancialsQueryHandler : IRequestHandler<GetProjectFinancialsQuery, List<MonthlyFinancialDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IForecastRepository _forecastRepository;
        private readonly IRosterRepository _rosterRepository;
        private readonly IBillingRepository _billingRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IOverrideRepository _overrideRepository;
        private readonly IGlobalRateRepository _globalRateRepository;
        private readonly IFinancialCalculationService _calculationService;

        public GetProjectFinancialsQueryHandler(
            IProjectRepository projectRepository,
            IForecastRepository forecastRepository,
            IRosterRepository rosterRepository,
            IBillingRepository billingRepository,
            IExpenseRepository expenseRepository,
            IOverrideRepository overrideRepository,
            IGlobalRateRepository globalRateRepository,
            IFinancialCalculationService calculationService)
        {
            _projectRepository = projectRepository;
            _forecastRepository = forecastRepository;
            _rosterRepository = rosterRepository;
            _billingRepository = billingRepository;
            _expenseRepository = expenseRepository;
            _overrideRepository = overrideRepository;
            _globalRateRepository = globalRateRepository;
            _calculationService = calculationService;
        }

        public async Task<List<MonthlyFinancialDto>> Handle(GetProjectFinancialsQuery request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null) throw new KeyNotFoundException("Project not found.");

            var allocations = await _forecastRepository.GetAllocationsByVersionAsync(request.ForecastVersionId);
            var rosterMembers = await _rosterRepository.GetByProjectVersionAsync(request.ForecastVersionId);

            var billings = await _billingRepository.GetByProjectAsync(request.ProjectId);
            var expenses = await _expenseRepository.GetByProjectAsync(request.ProjectId);
            var overrides = await _overrideRepository.GetByProjectAsync(request.ProjectId);
            var globalRates = await _globalRateRepository.GetAllAsync();

            return _calculationService.CalculateMonthlyFinancials(
                project,
                allocations,
                rosterMembers,
                billings,
                expenses,
                overrides,
                globalRates);
        }
    }
}
