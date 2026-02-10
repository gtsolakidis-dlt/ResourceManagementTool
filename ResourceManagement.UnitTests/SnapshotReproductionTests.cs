using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SnapshotReproductionTests
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

        public SnapshotReproductionTests()
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
        public async Task Recalculate_WhenPreviousMonthIsLocked_ShouldUpdateEditableMonth()
        {
            // Scenario: Jan is Locked. Feb is Editable.
            // User changes Feb Allocation.
            // Expectation: Feb Snapshot updates to reflect new Allocation.
            
            // Arrange
            int projectId = 1;
            int versionId = 1;
            DateTime fromMonth = new DateTime(2026, 2, 1); // Updating Feb

            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId))
                .ReturnsAsync(new Project { Id = projectId });

            // Mock Data
            var janDate = new DateTime(2026, 1, 1);
            var febDate = new DateTime(2026, 2, 1);

            // Jan is confirmed (locked)
            var janSnapshot = new ProjectMonthlySnapshot
            {
                Month = janDate,
                Status = SnapshotStatus.Confirmed,
                OpeningBalance = 0,
                Wip = 1000,
                CumulativeBillings = 1000,
                DirectExpenses = 100,
                OperationalCost = 800
            };

            // Feb is editable
            var febSnapshot = new ProjectMonthlySnapshot
            {
                Month = febDate,
                Status = SnapshotStatus.Editable,
                OpeningBalance = 0, // Should be propagated from Jan in reality, but logic will overwrite
                Wip = 2000, // Old value
            };

            // Snapshots in DB
            _mockSnapshotRepo.Setup(r => r.GetByProjectAsync(projectId, versionId))
                .ReturnsAsync(new List<ProjectMonthlySnapshot> { janSnapshot, febSnapshot });

            // Non-confirmed snapshots from Feb onwards -> Just Feb
            _mockSnapshotRepo.Setup(r => r.GetNonConfirmedFromMonthAsync(projectId, versionId, fromMonth))
                .ReturnsAsync(new List<ProjectMonthlySnapshot> { febSnapshot });

            // Simulated Calculation Service Output (New Allocation applied)
            // Jan is calculated as 1000 (Consistent with locked)
            // Feb is calculated as 2500 (Increased by 500 due to new allocation)
            var calculatedData = new List<MonthlyFinancialDto>
            {
                new MonthlyFinancialDto { Month = janDate, Wip = 1000, Cost = 800, Billings = 1000, Expenses=100 },
                new MonthlyFinancialDto { Month = febDate, Wip = 2500, Cost = 2000, Billings = 2000, Expenses=200 } 
            };

             _mockCalculationService.Setup(s => s.CalculateMonthlyFinancialsWithProjectRates(It.IsAny<Project>(), It.IsAny<List<ResourceAllocation>>(), It.IsAny<List<Roster>>(), It.IsAny<List<Billing>>(), It.IsAny<List<Expense>>(), It.IsAny<List<Override>>(), It.IsAny<List<ProjectRate>>()))
                .Returns(calculatedData);

            // Act
            await _service.RecalculateFromMonthAsync(projectId, versionId, fromMonth);

            // Assert
            // 1. janSnapshot is NOT updated (it wasn't in NonConfirmed list)
            _mockSnapshotRepo.Verify(r => r.UpdateAsync(It.Is<ProjectMonthlySnapshot>(s => s.Month == janDate)), Times.Never);

            // 2. febSnapshot IS updated
            // Wip should be Calc(Feb) + Delta.
            // Delta = Locked(Jan) - Calc(Jan) = 1000 - 1000 = 0.
            // Snapshot(Feb) = 2500 + 0 = 2500.
            _mockSnapshotRepo.Verify(r => r.UpdateAsync(It.Is<ProjectMonthlySnapshot>(s => 
                s.Month == febDate &&
                s.Wip == 2500
            )), Times.Once);
        }

        [Fact]
        public async Task Recalculate_WhenPreviousMonthIsLocked_AndChanged_ShouldNeutralizeChangeForEditableMonth()
        {
            // Scenario: Jan is Locked. Feb is Editable.
            // User changes Jan Allocation (which is locked!).
            // Expectation: Feb Snapshot does NOT change (or changes minimally), because Jan's change is neutralized by Delta.
            
            // Arrange
            int projectId = 1;
            int versionId = 1;
            DateTime fromMonth = new DateTime(2026, 1, 1); // Updating Jan (triggering recalc from Jan)

            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId))
                .ReturnsAsync(new Project { Id = projectId });

            // Mock Data
            var janDate = new DateTime(2026, 1, 1);
            var febDate = new DateTime(2026, 2, 1);

            // Jan is confirmed (locked) at Wip 1000
            var janSnapshot = new ProjectMonthlySnapshot
            {
                Month = janDate,
                Status = SnapshotStatus.Confirmed,
                Wip = 1000
            };

            // Feb is editable, currently at Wip 2000 (assuming consistent with Jan1000 + Feb1000)
            var febSnapshot = new ProjectMonthlySnapshot
            {
                Month = febDate,
                Status = SnapshotStatus.Editable,
                Wip = 2000
            };

            // Snapshots in DB
            _mockSnapshotRepo.Setup(r => r.GetByProjectAsync(projectId, versionId))
                .ReturnsAsync(new List<ProjectMonthlySnapshot> { janSnapshot, febSnapshot });

            // Non-confirmed snapshots from Jan -> Only Feb (Jan is confirmed)
            // Wait, GetNonConfirmedFromMonthAsync(Jan) should exclude Jan because it's Confirmed.
            // So it returns [Feb].
            _mockSnapshotRepo.Setup(r => r.GetNonConfirmedFromMonthAsync(projectId, versionId, fromMonth))
                .ReturnsAsync(new List<ProjectMonthlySnapshot> { febSnapshot });

            // Simulated Calculation Service Output (New Allocation applied to Jan)
            // User added 500 to Jan.
            // Jan Calc = 1500.
            // Feb Calc = 1500 (Jan) + 1000 (Feb) = 2500.
            var calculatedData = new List<MonthlyFinancialDto>
            {
                new MonthlyFinancialDto { Month = janDate, Wip = 1500 },
                new MonthlyFinancialDto { Month = febDate, Wip = 2500 } 
            };

             _mockCalculationService.Setup(s => s.CalculateMonthlyFinancialsWithProjectRates(It.IsAny<Project>(), It.IsAny<List<ResourceAllocation>>(), It.IsAny<List<Roster>>(), It.IsAny<List<Billing>>(), It.IsAny<List<Expense>>(), It.IsAny<List<Override>>(), It.IsAny<List<ProjectRate>>()))
                .Returns(calculatedData);

            // Act
            await _service.RecalculateFromMonthAsync(projectId, versionId, fromMonth);

            // Assert
            // 1. janSnapshot is NOT updated
            _mockSnapshotRepo.Verify(r => r.UpdateAsync(It.Is<ProjectMonthlySnapshot>(s => s.Month == janDate)), Times.Never);

            // 2. febSnapshot
            // Delta calculation:
            // Anchor = Jan (Confirmed).
            // Anchor.Wip (1000) - CalcData.Wip (1500) = -500.
            // Feb Snapshot Wip = CalcData.Wip (2500) + Delta (-500) = 2000.
            
            // So Feb Snapshot stays at 2000. The +500 in Jan is invisible.
            _mockSnapshotRepo.Verify(r => r.UpdateAsync(It.Is<ProjectMonthlySnapshot>(s => 
                s.Month == febDate &&
                s.Wip == 2000
            )), Times.Once);
        }

        [Fact]
        public async Task Recalculate_WhenAnchorIsZero_AndRatesAreAdded_ShouldResultInZeroWipForFuture()
        {
            // Scenario: Jan is Locked at 0 (e.g. no rates/allocations at the time).
            // User adds Allocations/Rates.
            // Jan (Calc) becomes 1000.
            // Delta = Locked(0) - Calc(1000) = -1000.
            // Feb (Calc) = 1200.
            // Feb Snapshot = 1200 + (-1000) = 200.
            // Result: Feb is heavily suppressed (showing only net diff), not true value.
            
            // Arrange
            int projectId = 1;
            int versionId = 1;
            DateTime fromMonth = new DateTime(2026, 2, 1);

            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId))
                .ReturnsAsync(new Project { Id = projectId });

            var janDate = new DateTime(2026, 1, 1);
            var febDate = new DateTime(2026, 2, 1);

            // Jan is confirmed at 0
            var janSnapshot = new ProjectMonthlySnapshot
            {
                Month = janDate,
                Status = SnapshotStatus.Confirmed,
                Wip = 0
            };

            var febSnapshot = new ProjectMonthlySnapshot
            {
                Month = febDate,
                Status = SnapshotStatus.Editable,
                Wip = 0
            };

            _mockSnapshotRepo.Setup(r => r.GetByProjectAsync(projectId, versionId))
                .ReturnsAsync(new List<ProjectMonthlySnapshot> { janSnapshot, febSnapshot });

            _mockSnapshotRepo.Setup(r => r.GetNonConfirmedFromMonthAsync(projectId, versionId, fromMonth))
                .ReturnsAsync(new List<ProjectMonthlySnapshot> { febSnapshot });

            // Calculated Data (Now reflecting real values)
            var calculatedData = new List<MonthlyFinancialDto>
            {
                new MonthlyFinancialDto { Month = janDate, Wip = 1000 },
                new MonthlyFinancialDto { Month = febDate, Wip = 1200 } 
            };

             _mockCalculationService.Setup(s => s.CalculateMonthlyFinancialsWithProjectRates(It.IsAny<Project>(), It.IsAny<List<ResourceAllocation>>(), It.IsAny<List<Roster>>(), It.IsAny<List<Billing>>(), It.IsAny<List<Expense>>(), It.IsAny<List<Override>>(), It.IsAny<List<ProjectRate>>()))
                .Returns(calculatedData);

            // Act
            await _service.RecalculateFromMonthAsync(projectId, versionId, fromMonth);

            // Assert
            // Feb should be Calc(1200) + Delta(0 - 1000).
            _mockSnapshotRepo.Verify(r => r.UpdateAsync(It.Is<ProjectMonthlySnapshot>(s => 
                s.Month == febDate &&
                s.Wip == 200 // 1200 - 1000
            )), Times.Once);
        }
    }
}
