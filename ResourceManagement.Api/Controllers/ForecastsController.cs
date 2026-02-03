using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ResourceManagement.Application.Forecasting.Commands.CloneVersion;
using ResourceManagement.Application.Forecasting.Commands.UpsertAllocations;
using ResourceManagement.Application.Forecasting.Queries.GetVersions;
using ResourceManagement.Application.Forecasting.Queries.GetAllocations;
using ResourceManagement.Contracts.Forecasting;
using ResourceManagement.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceManagement.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Basic")]
    [ApiController]
    [Route("api/[controller]")]
    public class ForecastsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IForecastRepository _forecastRepository;
        private readonly IProjectRepository _projectRepository;

        public ForecastsController(IMediator mediator, IForecastRepository forecastRepository, IProjectRepository projectRepository)
        {
            _mediator = mediator;
            _forecastRepository = forecastRepository;
            _projectRepository = projectRepository;
        }

        [HttpGet("{projectId}/versions")]
        public async Task<IActionResult> GetVersions(int projectId)
        {
            var versions = await _mediator.Send(new GetVersionsQuery(projectId));
            return Ok(versions);
        }

        [HttpGet("allocations/{versionId}")]
        public async Task<IActionResult> GetAllocations(int versionId)
        {
            var allocations = await _mediator.Send(new GetAllocationsQuery(versionId));
            return Ok(allocations);
        }

        [HttpPost("allocations")]
        public async Task<IActionResult> UpsertAllocations([FromBody] List<ResourceAllocationDto> allocations)
        {
            // RBAC: Check edit permissions
            var role = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var rosterIdStr = User.FindFirst("rosterId")?.Value ?? User.FindFirst("RosterId")?.Value;
            int.TryParse(rosterIdStr, out var userId);

            if (role == "Employee")
            {
                return Forbid(); // Employees cannot edit allocations
            }

            // For Manager, check assignment for each project involved
            if (role == "Manager" && userId > 0)
            {
                var versionIds = allocations.Select(a => a.ForecastVersionId).Distinct();
                foreach (var versionId in versionIds)
                {
                    var version = await _forecastRepository.GetVersionByIdAsync(versionId);
                    if (version != null)
                    {
                        var isAssigned = await _projectRepository.IsUserAssignedToProjectAsync(userId, version.ProjectId);
                        if (!isAssigned)
                        {
                            return Forbid(); // Not assigned to this project
                        }
                    }
                }
            }

            // Group allocations by ForecastVersionId and RosterId
            var groups = allocations.GroupBy(a => new { a.ForecastVersionId, a.RosterId });
            
            foreach (var group in groups)
            {
                var allocationDtos = group.Select(a => new AllocationUpsertDto(a.Month, a.AllocatedDays)).ToList();
                await _mediator.Send(new UpsertAllocationsCommand(
                    group.Key.ForecastVersionId,
                    group.Key.RosterId,
                    allocationDtos
                ));
            }
            
            
            return Ok();
        }

        [HttpDelete("allocations/{versionId}/{rosterId}")]
        public async Task<IActionResult> DeleteAllocation(int versionId, int rosterId)
        {
            // RBAC: Check edit permissions
            var role = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var rosterIdStr = User.FindFirst("rosterId")?.Value ?? User.FindFirst("RosterId")?.Value;
            int.TryParse(rosterIdStr, out var userId);

            if (role == "Employee")
            {
                return Forbid();
            }

            if (role == "Manager" && userId > 0)
            {
                var version = await _forecastRepository.GetVersionByIdAsync(versionId);
                if (version != null)
                {
                    var isAssigned = await _projectRepository.IsUserAssignedToProjectAsync(userId, version.ProjectId);
                    if (!isAssigned)
                    {
                        return Forbid();
                    }
                }
            }

            await _mediator.Send(new ResourceManagement.Application.Forecasting.Commands.DeleteResourceAllocation.DeleteResourceAllocationCommand(versionId, rosterId));
            return NoContent();
        }

        [HttpPost("clone")]
        public async Task<IActionResult> Clone([FromBody] CloneVersionCommand command)
        {
            // RBAC: Check edit permissions (clone requires edit access)
            var role = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var rosterIdStr = User.FindFirst("rosterId")?.Value ?? User.FindFirst("RosterId")?.Value;
            int.TryParse(rosterIdStr, out var userId);

            if (role == "Employee")
            {
                return Forbid();
            }

            if (role == "Manager" && userId > 0)
            {
                var version = await _forecastRepository.GetVersionByIdAsync(command.SourceVersionId);
                if (version != null)
                {
                    var isAssigned = await _projectRepository.IsUserAssignedToProjectAsync(userId, version.ProjectId);
                    if (!isAssigned)
                    {
                        return Forbid();
                    }
                }
            }

            var newId = await _mediator.Send(command);
            return Ok(new { Id = newId });
        }
    }
}

