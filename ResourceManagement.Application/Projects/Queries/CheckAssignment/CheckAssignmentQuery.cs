using MediatR;
using ResourceManagement.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Projects.Queries.CheckAssignment
{
    public record CheckAssignmentQuery(int RosterId, int ProjectId) : IRequest<bool>;

    public class CheckAssignmentQueryHandler : IRequestHandler<CheckAssignmentQuery, bool>
    {
        private readonly IProjectRepository _projectRepository;

        public CheckAssignmentQueryHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<bool> Handle(CheckAssignmentQuery request, CancellationToken cancellationToken)
        {
            return await _projectRepository.IsUserAssignedToProjectAsync(request.RosterId, request.ProjectId);
        }
    }
}
