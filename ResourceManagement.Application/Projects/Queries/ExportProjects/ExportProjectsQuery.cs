using MediatR;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.Project;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Projects.Queries.ExportProjects
{
    public record ExportProjectsQuery : IRequest<byte[]>;

    public class ExportProjectsQueryHandler : IRequestHandler<ExportProjectsQuery, byte[]>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IExcelService _excelService;

        public ExportProjectsQueryHandler(IProjectRepository projectRepository, IExcelService excelService)
        {
            _projectRepository = projectRepository;
            _excelService = excelService;
        }

        public async Task<byte[]> Handle(ExportProjectsQuery request, CancellationToken cancellationToken)
        {
            var projects = await _projectRepository.GetAllAsync();
            return await _excelService.ExportToExcelAsync(projects, "Projects");
        }
    }
}
