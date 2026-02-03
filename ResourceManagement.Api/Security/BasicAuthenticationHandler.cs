using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ResourceManagement.Api.Security
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ResourceManagement.Domain.Interfaces.IRosterRepository _repository;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ResourceManagement.Domain.Interfaces.IRosterRepository repository) : base(options, logger, encoder)
        {
            _repository = repository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                var username = credentials[0];
                var password = credentials[1];

                var user = await _repository.GetByUsernameAsync(username);

                // MVP: Simple password check (cleartext match)
                if (user != null && user.PasswordHash == password)
                {
                    var claims = new[] {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim("id", user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim("username", user.Username),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("role", user.Role),
                        new Claim("RosterId", user.Id.ToString()),
                        new Claim("rosterId", user.Id.ToString())
                    };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }

                // Fallback for bootstrap Admin (since DB might be empty of users with creds)
                if (username == "admin" && password == "Deloitte2026!")
                {
                    var claims = new[] {
                        new Claim(ClaimTypes.NameIdentifier, "0"), // Keep standard for Authorize attributes
                        new Claim("id", "0"),
                        new Claim("username", "admin"), // Use login username
                        new Claim("role", "Admin"),
                        new Claim("rosterId", "0") 
                    };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }
                
                return AuthenticateResult.Fail("Invalid Username or Password");
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        }
    }
}
