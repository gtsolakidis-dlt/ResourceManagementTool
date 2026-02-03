using MediatR;
using FluentValidation;
using ResourceManagement.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ResourceManagement.Application.GlobalRates.Commands.UpdateGlobalRate
{
    public record UpdateGlobalRateCommand : IRequest<Unit>
    {
        public int Id { get; init; }
        public string Level { get; init; } = string.Empty;
        public decimal NominalRate { get; init; }
    }

    public class UpdateGlobalRateCommandValidator : AbstractValidator<UpdateGlobalRateCommand>
    {
        public UpdateGlobalRateCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Level).NotEmpty().MaximumLength(50);
            RuleFor(x => x.NominalRate).GreaterThanOrEqualTo(0);
        }
    }

    public class UpdateGlobalRateCommandHandler : IRequestHandler<UpdateGlobalRateCommand, Unit>
    {
        private readonly IGlobalRateRepository _repository;

        public UpdateGlobalRateCommandHandler(IGlobalRateRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(UpdateGlobalRateCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                throw new Exception($"Global Rate with ID {request.Id} not found.");
            }

            // Check uniqueness if Level changed
            if (entity.Level != request.Level)
            {
                 var existing = await _repository.GetByLevelAsync(request.Level);
                 if (existing != null)
                 {
                     throw new Exception($"Rate for level {request.Level} already exists.");
                 }
            }

            entity.Level = request.Level;
            entity.NominalRate = request.NominalRate;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);
            return Unit.Value;
        }
    }
}
