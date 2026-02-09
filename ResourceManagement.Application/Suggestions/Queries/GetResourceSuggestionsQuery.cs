using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ResourceManagement.Contracts.Suggestions;
using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Application.Suggestions.Queries
{
    public record GetResourceSuggestionsQuery(int ProjectId, int ForecastVersionId) : IRequest<List<ResourceSuggestionDto>>;

    public class GetResourceSuggestionsQueryHandler : IRequestHandler<GetResourceSuggestionsQuery, List<ResourceSuggestionDto>>
    {
        private const decimal WorkingDaysPerMonth = 18m;

        private readonly IProjectRepository _projectRepository;
        private readonly IRosterRepository _rosterRepository;
        private readonly IForecastRepository _forecastRepository;

        public GetResourceSuggestionsQueryHandler(
            IProjectRepository projectRepository,
            IRosterRepository rosterRepository,
            IForecastRepository forecastRepository)
        {
            _projectRepository = projectRepository;
            _rosterRepository = rosterRepository;
            _forecastRepository = forecastRepository;
        }

        public async Task<List<ResourceSuggestionDto>> Handle(GetResourceSuggestionsQuery request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

            // Load all data in parallel
            var rosterTask = _rosterRepository.GetAllAsync();
            var allAllocationsTask = _forecastRepository.GetAllLatestAllocationsAsync();
            var currentAllocationsTask = _forecastRepository.GetAllocationsByVersionAsync(request.ForecastVersionId);

            await Task.WhenAll(rosterTask, allAllocationsTask, currentAllocationsTask);

            var allRoster = rosterTask.Result;
            var allLatestAllocations = allAllocationsTask.Result;
            var currentAllocations = currentAllocationsTask.Result;

            // Determine which roster members are already assigned in this version
            var assignedRosterIds = currentAllocations.Select(a => a.RosterId).Distinct().ToHashSet();

            // Generate project months
            var projectMonths = GenerateProjectMonths(project.StartDate, project.EndDate);

            // Build a lookup: RosterId -> Month -> total allocated days (across other projects)
            // Exclude the current project's latest version allocations to avoid double-counting
            var currentProjectVersionIds = (await _forecastRepository.GetByProjectAsync(request.ProjectId))
                .Select(v => v.Id).ToHashSet();

            var otherProjectAllocations = allLatestAllocations
                .Where(a => !currentProjectVersionIds.Contains(a.ForecastVersionId))
                .ToList();

            var allocationLookup = otherProjectAllocations
                .GroupBy(a => a.RosterId)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(a => a.Month.ToString("yyyy-MM-01"))
                          .ToDictionary(mg => mg.Key, mg => mg.Sum(a => a.AllocatedDays))
                );

            // Calculate remaining budget for the project
            var rosterLookup = allRoster.ToDictionary(r => r.Id);
            var currentAllocatedCost = currentAllocations
                .Where(a => rosterLookup.ContainsKey(a.RosterId))
                .Sum(a => a.AllocatedDays * rosterLookup[a.RosterId].DailyCost);
            var remainingBudget = project.ActualBudget - currentAllocatedCost;

            // Build suggestions for unassigned roster members
            var suggestions = new List<ResourceSuggestionDto>();

            foreach (var member in allRoster)
            {
                if (assignedRosterIds.Contains(member.Id))
                    continue;

                var monthlyAvailability = new List<MonthlyAvailabilityDto>();
                decimal totalAvailable = 0;
                decimal totalCapacity = 0;

                foreach (var month in projectMonths)
                {
                    var allocatedElsewhere = 0m;
                    if (allocationLookup.TryGetValue(member.Id, out var monthMap))
                    {
                        monthMap.TryGetValue(month, out allocatedElsewhere);
                    }

                    var available = Math.Max(0, WorkingDaysPerMonth - allocatedElsewhere);
                    totalAvailable += available;
                    totalCapacity += WorkingDaysPerMonth;

                    monthlyAvailability.Add(new MonthlyAvailabilityDto
                    {
                        Month = month,
                        AllocatedDays = allocatedElsewhere,
                        AvailableDays = available,
                        CapacityDays = WorkingDaysPerMonth
                    });
                }

                var availabilityPct = totalCapacity > 0 ? (totalAvailable / totalCapacity) * 100 : 0;
                var projectedCost = member.DailyCost * totalAvailable;
                var budgetImpactPct = remainingBudget > 0 ? (projectedCost / remainingBudget) * 100 : 100;

                string budgetFit;
                if (remainingBudget <= 0 || projectedCost > remainingBudget)
                    budgetFit = "over";
                else if (budgetImpactPct > 80)
                    budgetFit = "tight";
                else
                    budgetFit = "within";

                suggestions.Add(new ResourceSuggestionDto
                {
                    RosterId = member.Id,
                    FullNameEn = member.FullNameEn,
                    Level = member.Level,
                    TechnicalRole = member.TechnicalRole,
                    SeniorityTier = GetSeniorityTier(member.Level),
                    FunctionBusinessUnit = member.FunctionBusinessUnit,
                    DailyCost = member.DailyCost,
                    TotalAvailableDays = totalAvailable,
                    TotalCapacityDays = totalCapacity,
                    AvailabilityPercentage = Math.Round(availabilityPct, 1),
                    MonthlyAvailability = monthlyAvailability,
                    ProjectedCost = Math.Round(projectedCost, 2),
                    RemainingBudget = Math.Round(remainingBudget, 2),
                    BudgetImpactPercentage = Math.Round(budgetImpactPct, 1),
                    BudgetFit = budgetFit
                });
            }

            // Sort: availability DESC, then cost ASC
            return suggestions
                .OrderByDescending(s => s.AvailabilityPercentage)
                .ThenBy(s => s.DailyCost)
                .ToList();
        }

        private static string GetSeniorityTier(string? level) => level?.ToUpperInvariant() switch
        {
            "BA" => "Junior",
            "C" or "SC" => "Mid",
            "AM" or "M" => "Senior",
            "SM" or "D" or "P" => "Principal",
            _ => "Unknown"
        };

        private static List<string> GenerateProjectMonths(DateTime startDate, DateTime endDate)
        {
            var months = new List<string>();
            var current = new DateTime(startDate.Year, startDate.Month, 1);
            var end = new DateTime(endDate.Year, endDate.Month, 1);

            while (current <= end)
            {
                months.Add(current.ToString("yyyy-MM-01"));
                current = current.AddMonths(1);
            }

            return months;
        }
    }
}
