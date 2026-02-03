using MediatR;
using AutoMapper;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.Project;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Projects.Queries.GetProjectList
{
    public record GetProjectListQuery : IRequest<List<ProjectDto>>
    {
        public string? UserRole { get; init; }
        public int? UserId { get; init; }
    }

    public class GetProjectListQueryHandler : IRequestHandler<GetProjectListQuery, List<ProjectDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public GetProjectListQueryHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<List<ProjectDto>> Handle(GetProjectListQuery request, CancellationToken cancellationToken)
        {
            List<ResourceManagement.Domain.Entities.Project> projects;
            HashSet<int> assignedProjectIds = new HashSet<int>();

            // Get user's assigned projects for RBAC checks
            if (request.UserId.HasValue && request.UserId.Value > 0)
            {
                var assignedProjects = await _projectRepository.GetByResourceIdAsync(request.UserId.Value);
                assignedProjectIds = new HashSet<int>(assignedProjects.Select(p => p.Id));
            }

            if (request.UserRole == "Employee" && request.UserId.HasValue)
            {
                // Employees only see assigned projects
                projects = await _projectRepository.GetByResourceIdAsync(request.UserId.Value);
            }
            else
            {
                // Admin, Partner, Manager see all projects
                projects = await _projectRepository.GetAllAsync();
            }

            var result = _mapper.Map<List<ProjectDto>>(projects);

            // Set CanEdit based on role
            foreach (var dto in result)
            {
                if (request.UserRole == "Admin" || request.UserRole == "Partner")
                {
                    dto.CanEdit = true;
                }
                else if (request.UserRole == "Manager")
                {
                    dto.CanEdit = assignedProjectIds.Contains(dto.Id);
                }
                else // Employee
                {
                    dto.CanEdit = false;
                }
            }

            return result;
        }
    }
}
