using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ResourceManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ResourceManagement.Infrastructure.Persistence.DapperContext _context;

        public AuthController(ResourceManagement.Infrastructure.Persistence.DapperContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = "Basic")]
        public IActionResult Verify()
        {
            var userId = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst("username")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
            var rosterId = User.FindFirst("rosterId")?.Value ?? User.FindFirst("RosterId")?.Value;

            System.Console.WriteLine($"[Verify] User: {username}, Role Claim: '{role}'");

            return Ok(new { 
                Id = userId, 
                Username = username, 
                Role = role, // Ensure this sends the Exact string from the claim
                RosterId = rosterId
            });
        }

        [HttpPost("migrate")]
        [AllowAnonymous] // Allow running without login if broken, or require Admin? I'll allow anonymous to bootstrap
        public async Task<IActionResult> Migrate()
        {
            try 
            {
                using var connection = _context.CreateConnection();
                var sql = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Roster]') AND name = 'Username')
                    BEGIN
                        ALTER TABLE [dbo].[Roster] ADD [Username] NVARCHAR(100) NULL;
                        ALTER TABLE [dbo].[Roster] ADD [PasswordHash] NVARCHAR(255) NULL;
                        ALTER TABLE [dbo].[Roster] ADD [Role] NVARCHAR(50) DEFAULT 'Employee' WITH VALUES;
                    END
                ";
                
                await Dapper.SqlMapper.ExecuteAsync(connection, sql);
                return Ok("Migration Applied Successfully");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
