using MediatR;
using ResourceManagement.Domain.Interfaces;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Common.Behaviors
{
    public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IAuditRepository _auditRepository;

        public AuditBehavior(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Only audit Commands (by convention they end with "Command")
            if (!requestName.EndsWith("Command"))
            {
                return await next();
            }

            var response = await next();

            try
            {
                var payload = JsonSerializer.Serialize(request);
                await _auditRepository.LogAsync(
                    requestName,
                    "N/A", // In a real app, we'd extract an ID from the request or response
                    "Execute",
                    null,
                    payload,
                    "AuthenticatedUser" // In a real app, get from ICurrentUserService
                );
            }
            catch (Exception ex)
            {
                // Auditor should not crash the main thread, but we should log the failure
                Console.WriteLine($"Audit failure: {ex.Message}");
            }

            return response;
        }
    }
}
