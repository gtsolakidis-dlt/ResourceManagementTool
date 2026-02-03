using MediatR;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Forecasting.Queries.GetVersions
{
    public record GetVersionsQuery(int ProjectId) : IRequest<List<ForecastVersion>>;

    public class GetVersionsQueryHandler : IRequestHandler<GetVersionsQuery, List<ForecastVersion>>
    {
        private readonly IForecastRepository _forecastRepository;

        public GetVersionsQueryHandler(IForecastRepository forecastRepository)
        {
            _forecastRepository = forecastRepository;
        }

        public async Task<List<ForecastVersion>> Handle(GetVersionsQuery request, CancellationToken cancellationToken)
        {
            return await _forecastRepository.GetByProjectAsync(request.ProjectId);
        }

    }
}
