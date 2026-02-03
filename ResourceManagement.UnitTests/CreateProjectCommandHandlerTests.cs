using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Application.Projects.Commands.CreateProject;

namespace ResourceManagement.UnitTests
{
    public class CreateProjectCommandHandlerTests
    {
        private readonly Mock<IProjectRepository> _mockProjectRepo;
        private readonly Mock<IForecastRepository> _mockForecastRepo;
        private readonly Mock<IProjectRateRepository> _mockProjectRateRepo;
        private readonly Mock<IProjectMonthlySnapshotRepository> _mockSnapshotRepo;
        private readonly CreateProjectCommandHandler _handler;

        public CreateProjectCommandHandlerTests()
        {
            _mockProjectRepo = new Mock<IProjectRepository>();
            _mockForecastRepo = new Mock<IForecastRepository>();
            _mockProjectRateRepo = new Mock<IProjectRateRepository>();
            _mockSnapshotRepo = new Mock<IProjectMonthlySnapshotRepository>();

            _handler = new CreateProjectCommandHandler(
                _mockProjectRepo.Object,
                _mockForecastRepo.Object,
                _mockProjectRateRepo.Object,
                _mockSnapshotRepo.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldCalculateNominalBudgetCorrectly()
        {
            // Arrange
            var command = new CreateProjectCommand
            {
                Name = "Test Project",
                Wbs = "WBS.001",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                ActualBudget = 100000,
                Discount = 20
            };

            // Capture the saved project
            Project savedProject = null;
            _mockProjectRepo.Setup(r => r.CreateAsync(It.IsAny<Project>()))
                .Callback<Project>(p => savedProject = p)
                .ReturnsAsync(1);

            _mockProjectRepo.Setup(r => r.GetByWbsAsync(It.IsAny<string>()))
                .ReturnsAsync((Project)null); // Check for duplicate WBS returns null (ok)

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            savedProject.Should().NotBeNull();
            // 100k / (1 - 0.2) = 100k / 0.8 = 125k
            savedProject.NominalBudget.Should().Be(125000);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenWbsExists()
        {
            // Arrange
            var command = new CreateProjectCommand { Wbs = "EXISTING.WBS" };

            _mockProjectRepo.Setup(r => r.GetByWbsAsync("EXISTING.WBS"))
                .ReturnsAsync(new Project { Id = 1 });

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_ShouldInitializeSideEffects()
        {
             // Arrange
            var command = new CreateProjectCommand
            {
                Name = "Test Project",
                Wbs = "WBS.002",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                ActualBudget = 100000,
                Discount = 0
            };

            _mockProjectRepo.Setup(r => r.CreateAsync(It.IsAny<Project>())).ReturnsAsync(99);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            // 1. Should generate rates
            _mockProjectRateRepo.Verify(r => r.GenerateRatesForProjectAsync(99, 0), Times.Once);
            
            // 2. Should create forecast version
            _mockForecastRepo.Verify(r => r.CreateVersionAsync(It.Is<ForecastVersion>(fv => fv.ProjectId == 99 && fv.VersionNumber == 1)), Times.Once);

            // 3. Should initialize snapshots
            _mockSnapshotRepo.Verify(r => r.InitializeSnapshotsForProjectAsync(99, It.IsAny<int>(), command.StartDate, command.EndDate), Times.Once);
        }
    }
}
