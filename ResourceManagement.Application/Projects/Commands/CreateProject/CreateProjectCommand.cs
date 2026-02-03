using MediatR;
using FluentValidation;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Projects.Commands.CreateProject
{
    public record CreateProjectCommand : IRequest<int>
    {
        public string Name { get; init; } = string.Empty;
        public string Wbs { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public decimal ActualBudget { get; init; }
        public decimal Discount { get; init; } // Percentage 0-100
        public decimal Recoverability { get; init; }
        public decimal TargetMargin { get; init; }
    }

    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Wbs).NotEmpty().MaximumLength(100);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate);
            RuleFor(x => x.Discount).InclusiveBetween(0, 100);
        }
    }

    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, int>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IForecastRepository _forecastRepository;
        private readonly IProjectRateRepository _projectRateRepository;
        private readonly IProjectMonthlySnapshotRepository _snapshotRepository;

        public CreateProjectCommandHandler(
            IProjectRepository projectRepository,
            IForecastRepository forecastRepository,
            IProjectRateRepository projectRateRepository,
            IProjectMonthlySnapshotRepository snapshotRepository)
        {
            _projectRepository = projectRepository;
            _forecastRepository = forecastRepository;
            _projectRateRepository = projectRateRepository;
            _snapshotRepository = snapshotRepository;
        }

        public async Task<int> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            // Check for duplicate WBS
            var existing = await _projectRepository.GetByWbsAsync(request.Wbs);
            if (existing != null)
            {
                throw new ArgumentException($"Project with WBS {request.Wbs} already exists.");
            }

            var nominalBudget = request.ActualBudget;
            if (request.Discount < 100)
            {
                nominalBudget = request.ActualBudget / (1 - (request.Discount / 100m));
            }

            var project = new Project
            {
                Name = request.Name,
                Wbs = request.Wbs,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ActualBudget = request.ActualBudget,
                Discount = request.Discount,
                NominalBudget = nominalBudget,
                Recoverability = request.Recoverability,
                TargetMargin = request.TargetMargin
            };

            var projectId = await _projectRepository.CreateAsync(project);

            // Auto-generate ProjectRates from GlobalRates with project discount
            await _projectRateRepository.GenerateRatesForProjectAsync(projectId, request.Discount);

            // Auto-create Forecast Version 1
            var forecastVersion = new ForecastVersion
            {
                ProjectId = projectId,
                VersionNumber = 1
            };
            var forecastVersionId = await _forecastRepository.CreateVersionAsync(forecastVersion);

            // Auto-initialize monthly snapshots for the project date range
            // First month = Editable, subsequent months = Pending
            await _snapshotRepository.InitializeSnapshotsForProjectAsync(
                projectId,
                forecastVersionId,
                request.StartDate,
                request.EndDate);

            return projectId;
        }
    }
}
