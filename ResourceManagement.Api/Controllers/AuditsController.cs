using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ResourceManagement.Domain.Interfaces;
using System.Threading.Tasks;

namespace ResourceManagement.Api.Controllers
{
    [Authorize(AuthenticationSchemes = "Basic")]
    [ApiController]
    [Route("api/[controller]")]
    public class AuditsController : ControllerBase
    {
        private readonly IAuditRepository _auditRepository;

        public AuditsController(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent(int count = 10)
        {
            var audits = await _auditRepository.GetRecentAsync(count);
            return Ok(audits);
        }
    }
}
