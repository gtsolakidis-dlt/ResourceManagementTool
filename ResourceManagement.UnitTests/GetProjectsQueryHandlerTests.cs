using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Contracts.Project;
using ResourceManagement.Domain.Entities; 

namespace ResourceManagement.UnitTests
{
    public class GetProjectsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_Employee_ShouldReturnOnlyAssignedProjects()
        {
            // Arrange
            var userId = 1;
            var assignedProjects = new List<ResourceManagement.Domain.Entities.Project> 
            { 
                new ResourceManagement.Domain.Entities.Project { Id = 1, Name = "Assigned Project" } 
            };
            
            var mockRepo = new Mock<IProjectRepository>();
            mockRepo.Setup(r => r.GetByResourceIdAsync(userId))
                .ReturnsAsync(assignedProjects);

            var mockMapper = new Mock<IMapper>();
            // Simplify mapper setup
            mockMapper.Setup(m => m.Map<List<ProjectDto>>(It.IsAny<List<ResourceManagement.Domain.Entities.Project>>()))
                .Returns(new List<ProjectDto> { new ProjectDto { Id = 1, Name = "Assigned Project" } });

            var handler = new ResourceManagement.Application.Projects.Queries.GetProjectList.GetProjectListQueryHandler(mockRepo.Object, mockMapper.Object);

            var query = new ResourceManagement.Application.Projects.Queries.GetProjectList.GetProjectListQuery 
            { 
                UserRole = "Employee", 
                UserId = userId 
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal("Assigned Project", result[0].Name);
            Assert.False(result[0].CanEdit);
        }
    }
}
