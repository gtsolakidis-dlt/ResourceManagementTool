using MediatR;
using FluentValidation;
using ResourceManagement.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Projects.Commands.UpdateProject
{
    public record UpdateProjectCommand : IRequest<Unit>
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Wbs { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public decimal ActualBudget { get; init; }
        public decimal Discount { get; init; }
        public decimal Recoverability { get; init; }
        public decimal TargetMargin { get; init; }
    }

    public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Wbs).NotEmpty().MaximumLength(100);
        }
    }

    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Unit>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectRateRepository _projectRateRepository;

        public UpdateProjectCommandHandler(
            IProjectRepository projectRepository,
            IProjectRateRepository projectRateRepository)
        {
            _projectRepository = projectRepository;
            _projectRateRepository = projectRateRepository;
        }

        public async Task<Unit> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetByIdAsync(request.Id);
            if (project == null)
            {
                throw new Exception($"Project with ID {request.Id} not found.");
            }

            if (project.Wbs != request.Wbs)
            {
                var existing = await _projectRepository.GetByWbsAsync(request.Wbs);
                if (existing != null)
                {
                    throw new Exception($"WBS {request.Wbs} is already taken by another project.");
                }
            }

            // Track if discount changed for rate recalculation
            var discountChanged = project.Discount != request.Discount;

            var nominalBudget = request.ActualBudget;
            if (request.Discount < 100)
            {
                nominalBudget = request.ActualBudget / (1 - (request.Discount / 100m));
            }

            project.Name = request.Name;
            project.Wbs = request.Wbs;
            project.StartDate = request.StartDate;
            project.EndDate = request.EndDate;
            project.ActualBudget = request.ActualBudget;
            project.Discount = request.Discount;
            project.NominalBudget = nominalBudget;
            project.Recoverability = request.Recoverability;
            project.TargetMargin = request.TargetMargin;
            project.UpdatedAt = DateTime.UtcNow;

            await _projectRepository.UpdateAsync(project);

            // If discount changed, recalculate project rates
            // Note: This only affects non-Confirmed months (per design decision)
            if (discountChanged)
            {
                await _projectRateRepository.RecalculateRatesAsync(request.Id, request.Discount);
            }

            return Unit.Value;
        }
    }
}
