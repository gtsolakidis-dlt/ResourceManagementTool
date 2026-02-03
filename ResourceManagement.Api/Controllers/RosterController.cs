using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using ResourceManagement.Application.Roster.Commands.CreateRoster;
using ResourceManagement.Application.Roster.Commands.UpdateRoster;
using ResourceManagement.Application.Roster.Commands.DeleteRoster;
using ResourceManagement.Application.Roster.Queries.GetRoster;
using ResourceManagement.Application.Roster.Queries.GetRosterList;
using ResourceManagement.Application.Roster.Queries.ExportRoster;
using ResourceManagement.Application.Roster.Commands.ImportRoster;
using ResourceManagement.Contracts.Roster;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResourceManagement.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RosterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RosterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<RosterDto>>> GetList([FromQuery] GetRosterListQuery query)
        {
            return await _mediator.Send(query);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RosterDto>> Get(int id)
        {
            return await _mediator.Send(new GetRosterQuery(id));
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CreateRosterCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateRosterCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _mediator.Send(new DeleteRosterCommand(id));
            return NoContent();
        }
        [HttpGet("export")]
        public async Task<IActionResult> Export()
        {
            var file = await _mediator.Send(new ExportRosterQuery());
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Roster_{System.DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");
            using var stream = file.OpenReadStream();
            var count = await _mediator.Send(new ImportRosterCommand(stream));
            return Ok(new { Count = count });
        }
    }
}

