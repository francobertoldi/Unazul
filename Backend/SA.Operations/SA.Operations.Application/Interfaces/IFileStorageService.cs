namespace SA.Operations.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveDocumentAsync(Guid tenantId, Guid applicationId, Guid documentId, string originalFileName, Stream content, CancellationToken ct = default);
    Task DeleteDocumentAsync(string filePath, CancellationToken ct = default);
    Task<string?> GenerateSettlementExcelAsync(Guid tenantId, Guid settlementId, object summaryData, object itemsData, CancellationToken ct = default);
    Task<Stream?> GetFileStreamAsync(string filePath, CancellationToken ct = default);
}
