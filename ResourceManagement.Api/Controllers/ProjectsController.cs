using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ResourceManagement.Application.Projects.Commands.CreateProject;
using ResourceManagement.Application.Projects.Commands.UpdateProject;
using ResourceManagement.Application.Projects.Queries.GetProject;
using ResourceManagement.Application.Projects.Queries.GetProjectList;
using ResourceManagement.Application.Projects.Queries.ExportProjects;
using ResourceManagement.Application.Projects.Commands.ImportProjects;
using ResourceManagement.Contracts.Project;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResourceManagement.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProjectDto>>> GetList()
        {
            var role = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var rosterIdStr = User.FindFirst("rosterId")?.Value ?? User.FindFirst("RosterId")?.Value;
            int? rosterId = int.TryParse(rosterIdStr, out var rid) ? rid : null;

            return await _mediator.Send(new GetProjectListQuery { UserRole = role, UserId = rosterId });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> Get(int id)
        {
            var role = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var rosterIdStr = User.FindFirst("rosterId")?.Value ?? User.FindFirst("RosterId")?.Value;
            int? rosterId = int.TryParse(rosterIdStr, out var rid) ? rid : null;

            return await _mediator.Send(new GetProjectQuery(id, role, rosterId));
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CreateProjectCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateProjectCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            // RBAC: Check edit permissions
            var role = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var rosterIdStr = User.FindFirst("rosterId")?.Value ?? User.FindFirst("RosterId")?.Value;
            int.TryParse(rosterIdStr, out var rosterId);

            if (role == "Employee")
            {
                return Forbid(); // Employees cannot edit projects
            }

            if (role == "Manager" && rosterId > 0)
            {
                // Check if manager is assigned to this project
                var isAssigned = await _mediator.Send(new ResourceManagement.Application.Projects.Queries.CheckAssignment.CheckAssignmentQuery(rosterId, id));
                if (!isAssigned)
                {
                    return Forbid(); // Not assigned to this project
                }
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export()
        {
            var file = await _mediator.Send(new ExportProjectsQuery());
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Projects_{System.DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            using var stream = file.OpenReadStream();
            var count = await _mediator.Send(new ImportProjectsCommand(stream));
            return Ok(new { Count = count });
        }
    }
}

