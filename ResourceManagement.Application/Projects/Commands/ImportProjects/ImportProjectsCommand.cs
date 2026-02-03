using MediatR;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Domain.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ResourceManagement.Application.Projects.Commands.ImportProjects
{
    public record ImportProjectsCommand(Stream FileStream) : IRequest<int>;

    public class ImportProjectsCommandHandler : IRequestHandler<ImportProjectsCommand, int>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IExcelService _excelService;

        public ImportProjectsCommandHandler(IProjectRepository projectRepository, IExcelService excelService)
        {
            _projectRepository = projectRepository;
            _excelService = excelService;
        }

        public async Task<int> Handle(ImportProjectsCommand request, CancellationToken cancellationToken)
        {
            var importedData = await _excelService.ImportFromExcelAsync<ResourceManagement.Domain.Entities.Project>(request.FileStream);
            if (importedData == null || importedData.Count == 0) return 0;

            var existingProjects = await _projectRepository.GetAllAsync();
            var existingWbsCodes = existingProjects.ToDictionary(p => p.Wbs, p => p.Id);

            int count = 0;
            foreach (var project in importedData)
            {
                if (string.IsNullOrWhiteSpace(project.Wbs)) continue;
                
                // Ensure dates are valid for SQL (even with DateTime2, MinValue can be tricky)
                if (project.StartDate < new System.DateTime(1900, 1, 1)) project.StartDate = System.DateTime.Today;
                if (project.EndDate < project.StartDate) project.EndDate = project.StartDate.AddMonths(1);

                if (existingWbsCodes.TryGetValue(project.Wbs, out var existingId))
                {
                    project.Id = existingId;
                    project.UpdatedAt = System.DateTime.UtcNow;
                    await _projectRepository.UpdateAsync(project);
                }
                else
                {
                    project.CreatedAt = System.DateTime.UtcNow;
                    project.UpdatedAt = System.DateTime.UtcNow;
                    await _projectRepository.CreateAsync(project);
                }
                count++;
            }
            return count;
        }
    }
}
