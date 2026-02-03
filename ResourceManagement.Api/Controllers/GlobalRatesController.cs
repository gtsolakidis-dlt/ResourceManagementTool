using MediatR;
using Microsoft.AspNetCore.Mvc;
using ResourceManagement.Application.GlobalRates.Commands.CreateGlobalRate;
using ResourceManagement.Application.GlobalRates.Commands.UpdateGlobalRate;
using ResourceManagement.Application.GlobalRates.Queries.GetGlobalRates;
using ResourceManagement.Contracts.GlobalRates;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResourceManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GlobalRatesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GlobalRatesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<GlobalRateDto>>> GetAll()
        {
            var result = await _mediator.Send(new GetGlobalRatesQuery());
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CreateGlobalRateCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateGlobalRateCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }
    }
}
