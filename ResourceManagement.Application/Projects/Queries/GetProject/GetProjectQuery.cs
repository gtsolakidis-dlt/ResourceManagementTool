using MediatR;
using AutoMapper;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.Project;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Projects.Queries.GetProject
{
    public record GetProjectQuery(int Id, string? UserRole = null, int? UserId = null) : IRequest<ProjectDto>;

    public class GetProjectQueryHandler : IRequestHandler<GetProjectQuery, ProjectDto>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public GetProjectQueryHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<ProjectDto> Handle(GetProjectQuery request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetByIdAsync(request.Id);
            if (project == null)
            {
                throw new KeyNotFoundException($"Project with ID {request.Id} not found.");
            }

            var dto = _mapper.Map<ProjectDto>(project);

            // RBAC Logic
            if (request.UserRole == "Admin" || request.UserRole == "Partner")
            {
                dto.CanEdit = true;
            }
            else if (request.UserRole == "Manager" && request.UserId.HasValue)
            {
                dto.CanEdit = await _projectRepository.IsUserAssignedToProjectAsync(request.UserId.Value, request.Id);
            }
            else
            {
                dto.CanEdit = false;
            }

            return dto;
        }
    }
}
