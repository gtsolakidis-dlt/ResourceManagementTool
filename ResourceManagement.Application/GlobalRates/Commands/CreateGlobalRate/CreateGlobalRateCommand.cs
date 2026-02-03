using MediatR;
using FluentValidation;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ResourceManagement.Application.GlobalRates.Commands.CreateGlobalRate
{
    public record CreateGlobalRateCommand : IRequest<int>
    {
        public string Level { get; init; } = string.Empty;
        public decimal NominalRate { get; init; }
    }

    public class CreateGlobalRateCommandValidator : AbstractValidator<CreateGlobalRateCommand>
    {
        public CreateGlobalRateCommandValidator()
        {
            RuleFor(x => x.Level).NotEmpty().MaximumLength(50);
            RuleFor(x => x.NominalRate).GreaterThanOrEqualTo(0);
        }
    }

    public class CreateGlobalRateCommandHandler : IRequestHandler<CreateGlobalRateCommand, int>
    {
        private readonly IGlobalRateRepository _repository;

        public CreateGlobalRateCommandHandler(IGlobalRateRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateGlobalRateCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repository.GetByLevelAsync(request.Level);
            if (existing != null)
            {
                throw new Exception($"Rate for level {request.Level} already exists.");
            }

            var entity = new GlobalRate
            {
                Level = request.Level,
                NominalRate = request.NominalRate
            };

            return await _repository.CreateAsync(entity);
        }
    }
}
