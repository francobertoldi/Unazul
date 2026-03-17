using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SA.Operations.Application.Commands.Documents;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Xunit;
using AppEntity = SA.Operations.Domain.Entities.Application;

namespace SA.Operations.Tests.Unit.Commands.Documents;

public sealed class UploadDocumentCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository = Substitute.For<IApplicationRepository>();
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();
    private readonly UploadDocumentCommandHandler _sut;

    public UploadDocumentCommandHandlerTests()
    {
        _sut = new UploadDocumentCommandHandler(_applicationRepository, _documentRepository, _fileStorageService);
    }

    private static AppEntity CreateApplication(Guid tenantId)
    {
        return AppEntity.Create(tenantId, Guid.NewGuid(), Guid.NewGuid(), "OPS-001", Guid.NewGuid(), Guid.NewGuid(), "Product", "Plan", Guid.NewGuid());
    }

    [Fact]
    public async Task TP_OPS_11_01_UploadDocument_Successfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateApplication(tenantId);
        var createdBy = Guid.NewGuid();
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadDocumentCommand(app.Id, tenantId, "DNI Front", "Identity", "dni_front.pdf", stream, createdBy);

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>())
            .Returns(app);

        _fileStorageService.SaveDocumentAsync(
                tenantId, app.Id, Arg.Any<Guid>(), "dni_front.pdf", stream, Arg.Any<CancellationToken>())
            .Returns("https://storage.example.com/docs/dni_front.pdf");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("DNI Front");
        result.FileUrl.Should().Be("https://storage.example.com/docs/dni_front.pdf");

        await _documentRepository.Received(1).AddAsync(Arg.Any<ApplicationDocument>(), Arg.Any<CancellationToken>());
        await _documentRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TP_OPS_11_02_ThrowsOpsApplicationNotFound_WhenMissing()
    {
        // Arrange
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadDocumentCommand(Guid.NewGuid(), Guid.NewGuid(), "DNI Front", "Identity", "dni_front.pdf", stream, Guid.NewGuid());

        _applicationRepository.GetByIdAsync(command.ApplicationId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OPS_APPLICATION_NOT_FOUND");
    }

    [Fact]
    public async Task TP_OPS_11_03_CallsSaveDocumentAsync()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var app = CreateApplication(tenantId);
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadDocumentCommand(app.Id, tenantId, "DNI Front", "Identity", "dni_front.pdf", stream, Guid.NewGuid());

        _applicationRepository.GetByIdAsync(app.Id, Arg.Any<CancellationToken>())
            .Returns(app);

        _fileStorageService.SaveDocumentAsync(
                tenantId, app.Id, Arg.Any<Guid>(), "dni_front.pdf", stream, Arg.Any<CancellationToken>())
            .Returns("https://storage.example.com/docs/dni_front.pdf");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        await _fileStorageService.Received(1).SaveDocumentAsync(
            tenantId,
            app.Id,
            Arg.Any<Guid>(),
            "dni_front.pdf",
            stream,
            Arg.Any<CancellationToken>());
    }
}
