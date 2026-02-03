using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using ResourceManagement.Domain.Entities;
using ResourceManagement.Domain.Interfaces;
using ResourceManagement.Application.Roster.Commands.ImportRoster;

namespace ResourceManagement.UnitTests
{
    public class ImportRosterCommandHandlerTests
    {
        private readonly Mock<IRosterRepository> _mockRosterRepo;
        private readonly Mock<IExcelService> _mockExcelService;
        private readonly ImportRosterCommandHandler _handler;

        public ImportRosterCommandHandlerTests()
        {
            _mockRosterRepo = new Mock<IRosterRepository>();
            _mockExcelService = new Mock<IExcelService>();
            
            _handler = new ImportRosterCommandHandler(_mockRosterRepo.Object, _mockExcelService.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateExistingAndCreateNew_BasedOnSapCode()
        {
            // Arrange
            // Existing members
            var existingMembers = new List<Roster>
            {
                new Roster { Id = 1, SapCode = "SAP100", FullNameEn = "Old Name" }
            };
            _mockRosterRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(existingMembers);

            // Imported data
            var importedData = new List<Roster>
            {
                new Roster { SapCode = "SAP100", FullNameEn = "New Name" }, // Should Update
                new Roster { SapCode = "SAP200", FullNameEn = "John Doe" }  // Should Create
            };
            
            _mockExcelService.Setup(s => s.ImportFromExcelAsync<Roster>(It.IsAny<Stream>()))
                .ReturnsAsync(importedData);

            var command = new ImportRosterCommand(new MemoryStream());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(2);

            // Verify Update called for SAP100
            _mockRosterRepo.Verify(r => r.UpdateAsync(It.Is<Roster>(m => 
                m.Id == 1 && 
                m.SapCode == "SAP100" && 
                m.FullNameEn == "New Name"
            )), Times.Once);

             // Verify Create called for SAP200
            _mockRosterRepo.Verify(r => r.CreateAsync(It.Is<Roster>(m => 
                m.SapCode == "SAP200" && 
                m.FullNameEn == "John Doe"
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldSkip_IfSapCodeIsMissing()
        {
            // Arrange
            _mockRosterRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Roster>());
            
             var importedData = new List<Roster>
            {
                new Roster { SapCode = "", FullNameEn = "Invalid" }, // Skip
                new Roster { SapCode = null, FullNameEn = "Invalid 2" }  // Skip
            };
            
             _mockExcelService.Setup(s => s.ImportFromExcelAsync<Roster>(It.IsAny<Stream>()))
                .ReturnsAsync(importedData);

            var command = new ImportRosterCommand(new MemoryStream());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(0);
            _mockRosterRepo.Verify(r => r.CreateAsync(It.IsAny<Roster>()), Times.Never);
            _mockRosterRepo.Verify(r => r.UpdateAsync(It.IsAny<Roster>()), Times.Never);
        }
    }
}
