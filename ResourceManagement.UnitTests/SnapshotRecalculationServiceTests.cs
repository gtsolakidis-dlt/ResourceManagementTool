using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Domain.Services;
using ResourceManagement.Contracts.Financials;

namespace ResourceManagement.UnitTests
{
    public class SnapshotRecalculationServiceTests
    {
        private readonly Mock<IProjectMonthlySnapshotRepository> _mockSnapshotRepo;
        private readonly Mock<IFinancialCalculationService> _mockCalculationService;
        private readonly Mock<IProjectRepository> _mockProjectRepo;
        private readonly Mock<IForecastRepository> _mockForecastRepo;
        private readonly Mock<IRosterRepository> _mockRosterRepo;
        private readonly Mock<IProjectRateRepository> _mockProjectRateRepo;
        private readonly Mock<IBillingRepository> _mockBillingRepo;
        private readonly Mock<IExpenseRepository> _mockExpenseRepo;
        private readonly Mock<IOverrideRepository> _mockOverrideRepo;

        private readonly SnapshotRecalculationService _service;

        public SnapshotRecalculationServiceTests()
        {
            _mockSnapshotRepo = new Mock<IProjectMonthlySnapshotRepository>();
            _mockCalculationService = new Mock<IFinancialCalculationService>();
            _mockProjectRepo = new Mock<IProjectRepository>();
            _mockForecastRepo = new Mock<IForecastRepository>();
            _mockRosterRepo = new Mock<IRosterRepository>();
            _mockProjectRateRepo = new Mock<IProjectRateRepository>();
            _mockBillingRepo = new Mock<IBillingRepository>();
            _mockExpenseRepo = new Mock<IExpenseRepository>();
            _mockOverrideRepo = new Mock<IOverrideRepository>();

            _service = new SnapshotRecalculationService(
                _mockSnapshotRepo.Object,
                _mockCalculationService.Object,
                _mockProjectRepo.Object,
                _mockForecastRepo.Object,
                _mockRosterRepo.Object,
                _mockProjectRateRepo.Object,
                _mockBillingRepo.Object,
                _mockExpenseRepo.Object,
                _mockOverrideRepo.Object
            );
        }

        [Fact]
        public async Task RecalculateFromMonthAsync_ShouldUpdateSnapshots_WhenNoOverrides()
        {
            // Arrange
            int projectId = 1;
            int versionId = 1;
            DateTime fromMonth = new DateTime(2026, 1, 1);

            // Mock Project
            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId))
                .ReturnsAsync(new Project { Id = projectId });

            // Mock Calculation Service Result
            var calculatedData = new List<MonthlyFinancialDto>
            {
                new MonthlyFinancialDto
                {
                    Month = new DateTime(2026, 1, 1),
                    Billings = 1000, // Cumulative
                    Wip = 2000,
                    Expenses = 500,
                    Cost = 1500,
                    MonthlyBillings = 1000,
                    MonthlyExpenses = 500
                },
                new MonthlyFinancialDto
                {
                    Month = new DateTime(2026, 2, 1),
                    Billings = 2000, // Cumulative
                    Wip = 4000,
                    Expenses = 1000,
                    Cost = 3000,
                    MonthlyBillings = 1000,
                    MonthlyExpenses = 500
                }
            };

            _mockCalculationService.Setup(s => s.CalculateMonthlyFinancialsWithProjectRates(
                It.IsAny<Project>(),
                It.IsAny<List<ResourceAllocation>>(),
                It.IsAny<List<Roster>>(),
                It.IsAny<List<Billing>>(),
                It.IsAny<List<Expense>>(),
                It.IsAny<List<Override>>(),
                It.IsAny<List<ProjectRate>>()))
                .Returns(calculatedData);

            // Mock Snapshots to be updated
            var snapshots = new List<ProjectMonthlySnapshot>
            {
                new ProjectMonthlySnapshot { Month = new DateTime(2026, 1, 1), Status = SnapshotStatus.Pending, IsOverridden = false },
                new ProjectMonthlySnapshot { Month = new DateTime(2026, 2, 1), Status = SnapshotStatus.Pending, IsOverridden = false }
            };

            _mockSnapshotRepo.Setup(r => r.GetNonConfirmedFromMonthAsync(projectId, versionId, fromMonth))
                .ReturnsAsync(snapshots);
            
            _mockSnapshotRepo.Setup(r => r.GetByProjectAsync(projectId, versionId))
                .ReturnsAsync(snapshots); // For finding anchor (none here)

            // Act
            await _service.RecalculateFromMonthAsync(projectId, versionId, fromMonth);

            // Assert
            _mockSnapshotRepo.Verify(r => r.UpdateAsync(It.Is<ProjectMonthlySnapshot>(s => 
                s.Month.Month == 1 &&
                s.CumulativeBillings == 1000 &&
                s.Wip == 2000 &&
                s.DirectExpenses == 500 &&
                s.OperationalCost == 1500 &&
                s.Nsr == (2000 + 1000 - 0 - 500) // WIP + Billings - OB - Expenses = 2500
            )), Times.Once);

            _mockSnapshotRepo.Verify(r => r.UpdateAsync(It.Is<ProjectMonthlySnapshot>(s => 
                s.Month.Month == 2 &&
                s.CumulativeBillings == 2000 &&
                s.Wip == 4000
            )), Times.Once);
        }

        [Fact]
        public async Task RecalculateFromMonthAsync_ShouldPreserveOverrides_ButRecalculateNSR()
        {
             // Arrange
            int projectId = 1;
            int versionId = 1;
            DateTime fromMonth = new DateTime(2026, 1, 1);

            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId))
                .ReturnsAsync(new Project { Id = projectId });

            // Calculation service says WIP should be 2000
            var calculatedData = new List<MonthlyFinancialDto>
            {
                new MonthlyFinancialDto
                {
                    Month = new DateTime(2026, 1, 1),
                    Wip = 2000, 
                    MonthlyBillings = 500,
                    MonthlyExpenses = 100
                }
            };
            
             _mockCalculationService.Setup(s => s.CalculateMonthlyFinancialsWithProjectRates(It.IsAny<Project>(), It.IsAny<List<ResourceAllocation>>(), It.IsAny<List<Roster>>(), It.IsAny<List<Billing>>(), It.IsAny<List<Expense>>(), It.IsAny<List<Override>>(), It.IsAny<List<ProjectRate>>()))
                .Returns(calculatedData);

            // Existing snapshot has OVERRIDDEN WIP of 5000
            var snapshots = new List<ProjectMonthlySnapshot>
            {
                new ProjectMonthlySnapshot 
                { 
                    Month = new DateTime(2026, 1, 1), 
                    Status = SnapshotStatus.Editable, 
                    IsOverridden = true,
                    Wip = 5000, // OVERRIDE VALUE
                    OpeningBalance = 0,
                    CumulativeBillings = 1000,
                    DirectExpenses = 200,
                    OperationalCost = 4000
                }
            };

            _mockSnapshotRepo.Setup(r => r.GetNonConfirmedFromMonthAsync(projectId, versionId, fromMonth))
                .ReturnsAsync(snapshots);
            
            _mockSnapshotRepo.Setup(r => r.GetByProjectAsync(projectId, versionId))
                .ReturnsAsync(snapshots);

            // Act
            await _service.RecalculateFromMonthAsync(projectId, versionId, fromMonth);

            // Assert
            _mockSnapshotRepo.Verify(r => r.UpdateAsync(It.Is<ProjectMonthlySnapshot>(s => 
                s.Month.Month == 1 &&
                s.Wip == 5000 && // Should KEEP override
                s.Nsr == (5000 + 1000 - 0 - 200) // 5800. Re-calculated using overridden WIP!
            )), Times.Once);
        }

        [Fact]
        public async Task RecalculateFromMonthAsync_ShouldPropagateOpeningBalance_FromAnchor()
        {
            // Arrange
            int projectId = 1;
            int versionId = 1;
            DateTime fromMonth = new DateTime(2026, 2, 1); // Start from Feb

            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId))
                .ReturnsAsync(new Project { Id = projectId });

            var calculatedData = new List<MonthlyFinancialDto>
            {
                new MonthlyFinancialDto { Month = new DateTime(2026, 2, 1), Wip=100 },
                new MonthlyFinancialDto { Month = new DateTime(2026, 3, 1), Wip=100 }
            };
            
             _mockCalculationService.Setup(s => s.CalculateMonthlyFinancialsWithProjectRates(It.IsAny<Project>(), It.IsAny<List<ResourceAllocation>>(), It.IsAny<List<Roster>>(), It.IsAny<List<Billing>>(), It.IsAny<List<Expense>>(), It.IsAny<List<Override>>(), It.IsAny<List<ProjectRate>>()))
                .Returns(calculatedData);

            // Anchor snapshot (Jan) is Confirmed and has OpeningBalance = 999
            var janSnapshot = new ProjectMonthlySnapshot 
            { 
                Month = new DateTime(2026, 1, 1), 
                Status = SnapshotStatus.Confirmed,
                OpeningBalance = 999
            };
            
            // Snapshots to update
            var febSnapshot = new ProjectMonthlySnapshot { Month = new DateTime(2026, 2, 1), Status = SnapshotStatus.Pending };
            var marSnapshot = new ProjectMonthlySnapshot { Month = new DateTime(2026, 3, 1), Status = SnapshotStatus.Pending };

            _mockSnapshotRepo.Setup(r => r.GetByProjectAsync(projectId, versionId))
                .ReturnsAsync(new List<ProjectMonthlySnapshot> { janSnapshot, febSnapshot, marSnapshot });

            _mockSnapshotRepo.Setup(r => r.GetNonConfirmedFromMonthAsync(projectId, versionId, fromMonth))
                .ReturnsAsync(new List<ProjectMonthlySnapshot> { febSnapshot, marSnapshot });

            // Act
            await _service.RecalculateFromMonthAsync(projectId, versionId, fromMonth);

            // Assert
            // Feb should get OB from Jan (999)
            _mockSnapshotRepo.Verify(r => r.UpdateAsync(It.Is<ProjectMonthlySnapshot>(s => 
                s.Month.Month == 2 &&
                s.OpeningBalance == 999
            )), Times.Once);
        }
    }
}
