using MediatR;
using FluentValidation;
using RosterEntity = ResourceManagement.Domain.Entities.Roster;
using ResourceManagement.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Roster.Commands.UpdateRoster
{
    public record UpdateRosterCommand : IRequest<Unit>
    {
        public int Id { get; init; }
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
        public string? Username { get; init; }
        public string? Role { get; init; }
        public string? Password { get; init; }
    }

    public class UpdateRosterCommandValidator : AbstractValidator<UpdateRosterCommand>
    {
        public UpdateRosterCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.SapCode).NotEmpty().MaximumLength(50);
            RuleFor(x => x.FullNameEn).NotEmpty().MaximumLength(200);
            RuleFor(x => x.MonthlySalary).GreaterThanOrEqualTo(0);
        }
    }

    public class UpdateRosterCommandHandler : IRequestHandler<UpdateRosterCommand, Unit>
    {
        private readonly IRosterRepository _rosterRepository;

        public UpdateRosterCommandHandler(IRosterRepository rosterRepository)
        {
            _rosterRepository = rosterRepository;
        }

        public async Task<Unit> Handle(UpdateRosterCommand request, CancellationToken cancellationToken)
        {
            var roster = await _rosterRepository.GetByIdAsync(request.Id);
            if (roster == null)
            {
                throw new Exception($"Roster member with ID {request.Id} not found.");
            }

            // Check if SAP code is changed and if new code already exists
            if (roster.SapCode != request.SapCode)
            {
                var existing = await _rosterRepository.GetBySapCodeAsync(request.SapCode);
                if (existing != null)
                {
                    throw new Exception($"SAP Code {request.SapCode} is already taken by another member.");
                }
            }

            // Check username uniqueness if changed
            if (!string.IsNullOrEmpty(request.Username) && roster.Username != request.Username)
            {
                 var userExists = await _rosterRepository.GetByUsernameAsync(request.Username);
                 if (userExists != null)
                 {
                     throw new Exception($"Username {request.Username} is already taken.");
                 }
                 roster.Username = request.Username;
            }

            if (!string.IsNullOrEmpty(request.Role))
            {
                roster.Role = request.Role;
            }

            if (!string.IsNullOrEmpty(request.Password))
            {
                // In production use hashing
                roster.PasswordHash = request.Password; 
            }

            roster.SapCode = request.SapCode;
            roster.FullNameEn = request.FullNameEn;
            roster.LegalEntity = request.LegalEntity;
            roster.FunctionBusinessUnit = request.FunctionBusinessUnit;
            roster.CostCenterCode = request.CostCenterCode;
            roster.Level = request.Level;
            roster.MonthlySalary = request.MonthlySalary;
            roster.MonthlyEmployerContributions = request.MonthlyEmployerContributions;
            roster.Cars = request.Cars;
            roster.TicketRestaurant = request.TicketRestaurant;
            roster.Metlife = request.Metlife;
            roster.UpdatedAt = DateTime.UtcNow;

            await _rosterRepository.UpdateAsync(roster);
            return Unit.Value;
        }
    }
}
