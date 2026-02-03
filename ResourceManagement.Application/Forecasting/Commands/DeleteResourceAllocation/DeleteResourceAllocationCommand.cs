using MediatR;
using ResourceManagement.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Forecasting.Commands.DeleteResourceAllocation
{
    public record DeleteResourceAllocationCommand(int ForecastVersionId, int RosterId) : IRequest;

    public class DeleteResourceAllocationCommandHandler : IRequestHandler<DeleteResourceAllocationCommand>
    {
        private readonly IForecastRepository _forecastRepository;

        public DeleteResourceAllocationCommandHandler(IForecastRepository forecastRepository)
        {
            _forecastRepository = forecastRepository;
        }

        public async Task Handle(DeleteResourceAllocationCommand request, CancellationToken cancellationToken)
        {
            await _forecastRepository.DeleteResourceAllocationsAsync(request.ForecastVersionId, request.RosterId);
        }
    }
}
