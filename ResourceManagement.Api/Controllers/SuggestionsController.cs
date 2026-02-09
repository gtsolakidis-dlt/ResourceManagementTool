using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ResourceManagement.Application.Suggestions.Queries;

namespace ResourceManagement.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Basic")]
    [ApiController]
    [Route("api/[controller]")]
    public class SuggestionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SuggestionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("resources")]
        public async Task<IActionResult> GetResourceSuggestions(
            [FromQuery] int projectId,
            [FromQuery] int forecastVersionId)
        {
            var suggestions = await _mediator.Send(new GetResourceSuggestionsQuery(projectId, forecastVersionId));
            return Ok(suggestions);
        }
    }
}
