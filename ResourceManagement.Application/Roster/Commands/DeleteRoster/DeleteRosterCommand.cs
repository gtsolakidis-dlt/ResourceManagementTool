using MediatR;
using ResourceManagement.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Roster.Commands.DeleteRoster
{
    public record DeleteRosterCommand(int Id) : IRequest<Unit>;

    public class DeleteRosterCommandHandler : IRequestHandler<DeleteRosterCommand, Unit>
    {
        private readonly IRosterRepository _rosterRepository;

        public DeleteRosterCommandHandler(IRosterRepository rosterRepository)
        {
            _rosterRepository = rosterRepository;
        }

        public async Task<Unit> Handle(DeleteRosterCommand request, CancellationToken cancellationToken)
        {
            var roster = await _rosterRepository.GetByIdAsync(request.Id);
            if (roster == null)
            {
                throw new Exception($"Roster member with ID {request.Id} not found.");
            }

            await _rosterRepository.DeleteAsync(request.Id);
            return Unit.Value;
        }
    }
}
