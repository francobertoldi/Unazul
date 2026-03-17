using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SA.Operations.Application.Interfaces;
using SA.Operations.Domain.Entities;
using SA.Operations.Infrastructure.Options;
using Shared.Export;

namespace SA.Operations.Infrastructure.Services;

public sealed class FileStorageService : IFileStorageService
{
    private readonly StorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IOptions<StorageSettings> settings, ILogger<FileStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> SaveDocumentAsync(
        Guid tenantId,
        Guid applicationId,
        Guid documentId,
        string originalFileName,
        Stream content,
        CancellationToken ct = default)
    {
        var extension = Path.GetExtension(originalFileName);
        var relativePath = Path.Combine(
            tenantId.ToString(),
            "applications",
            applicationId.ToString(),
            "documents",
            $"{documentId}{extension}");

        var fullPath = Path.Combine(_settings.RootPath, relativePath);
        var directory = Path.GetDirectoryName(fullPath)!;

        Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, ct);

        _logger.LogInformation("Document saved: {Path}", relativePath);
        return relativePath;
    }

    public Task DeleteDocumentAsync(string filePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_settings.RootPath, filePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Document deleted: {Path}", filePath);
        }
        else
        {
            _logger.LogWarning("Document not found for deletion: {Path}", filePath);
        }

        return Task.CompletedTask;
    }

    public async Task<string?> GenerateSettlementExcelAsync(
        Guid tenantId,
        Guid settlementId,
        object summaryData,
        object itemsData,
        CancellationToken ct = default)
    {
        try
        {
            var items = (ICollection<SettlementItem>)itemsData;
            var totals = (ICollection<SettlementTotal>)summaryData;

            var columns = new List<ColumnDefinition<SettlementItem>>
            {
                new("Codigo", i => i.AppCode),
                new("Solicitante", i => i.ApplicantName),
                new("Producto", i => i.ProductName),
                new("Plan", i => i.PlanName),
                new("Tipo Comision", i => i.CommissionType),
                new("Valor Comision", i => i.CommissionValue),
                new("Monto Calculado", i => i.CalculatedAmount),
                new("Moneda", i => i.Currency),
                new("Formula", i => i.FormulaDescription)
            };

            var excelBytes = ExportService.ToXlsx(items, columns, "Liquidacion");

            var relativePath = Path.Combine(
                tenantId.ToString(),
                "settlements",
                $"liquidacion_{settlementId}.xlsx");

            var fullPath = Path.Combine(_settings.RootPath, relativePath);
            var directory = Path.GetDirectoryName(fullPath)!;

            Directory.CreateDirectory(directory);

            await File.WriteAllBytesAsync(fullPath, excelBytes, ct);

            _logger.LogInformation("Settlement Excel generated: {Path}", relativePath);
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate settlement Excel for {SettlementId}", settlementId);
            return null;
        }
    }

    public Task<Stream?> GetFileStreamAsync(string filePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_settings.RootPath, filePath);

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("File not found: {Path}", filePath);
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }
}
