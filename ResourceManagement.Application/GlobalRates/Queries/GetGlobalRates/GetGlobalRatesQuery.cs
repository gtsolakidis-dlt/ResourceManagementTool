using MediatR;
using AutoMapper;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.GlobalRates;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.GlobalRates.Queries.GetGlobalRates
{
    public record GetGlobalRatesQuery : IRequest<List<GlobalRateDto>>;

    public class GetGlobalRatesQueryHandler : IRequestHandler<GetGlobalRatesQuery, List<GlobalRateDto>>
    {
        private readonly IGlobalRateRepository _repository;
        private readonly IMapper _mapper;

        public GetGlobalRatesQueryHandler(IGlobalRateRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<GlobalRateDto>> Handle(GetGlobalRatesQuery request, CancellationToken cancellationToken)
        {
            var rates = await _repository.GetAllAsync();
            return _mapper.Map<List<GlobalRateDto>>(rates);
        }
    }
}
