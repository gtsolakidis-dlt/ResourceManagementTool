using MediatR;
using FluentValidation;
using RosterEntity = ResourceManagement.Domain.Entities.Roster;
using ResourceManagement.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Roster.Commands.CreateRoster
{
    public record CreateRosterCommand : IRequest<int>
    {
        public string SapCode { get; init; } = string.Empty;
        public string FullNameEn { get; init; } = string.Empty;
        public string? LegalEntity { get; init; }
        public string? FunctionBusinessUnit { get; init; }
        public string? CostCenterCode { get; init; }
        public string? Level { get; init; }
        public decimal MonthlySalary { get; init; }
        public decimal MonthlyEmployerContributions { get; init; }
        public decimal Cars { get; init; }
        public decimal TicketRestaurant { get; init; }
        public decimal Metlife { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }

    public class CreateRosterCommandValidator : AbstractValidator<CreateRosterCommand>
    {
        public CreateRosterCommandValidator()
        {
            RuleFor(x => x.SapCode).NotEmpty().MaximumLength(50);
            RuleFor(x => x.FullNameEn).NotEmpty().MaximumLength(200);
            RuleFor(x => x.MonthlySalary).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(4);
        }
    }

    public class CreateRosterCommandHandler : IRequestHandler<CreateRosterCommand, int>
    {
        private readonly IRosterRepository _rosterRepository;

        public CreateRosterCommandHandler(IRosterRepository rosterRepository)
        {
            _rosterRepository = rosterRepository;
        }

        public async Task<int> Handle(CreateRosterCommand request, CancellationToken cancellationToken)
        {
            // Check for duplicate SAP Code
            var existing = await _rosterRepository.GetBySapCodeAsync(request.SapCode);
            if (existing != null)
            {
                throw new ArgumentException($"SAP Code {request.SapCode} already exists.");
            }

            // TODO: Check for duplicate Username

            var roster = new RosterEntity
            {
                SapCode = request.SapCode,
                FullNameEn = request.FullNameEn,
                LegalEntity = request.LegalEntity,
                FunctionBusinessUnit = request.FunctionBusinessUnit,
                CostCenterCode = request.CostCenterCode,
                Level = request.Level,
                MonthlySalary = request.MonthlySalary,
                MonthlyEmployerContributions = request.MonthlyEmployerContributions,
                Cars = request.Cars,
                TicketRestaurant = request.TicketRestaurant,
                Metlife = request.Metlife,
                Role = "Employee",
                Username = request.Username,
                PasswordHash = request.Password // In real app use BCrypt
            };

            return await _rosterRepository.CreateAsync(roster);
        }
    }
}
