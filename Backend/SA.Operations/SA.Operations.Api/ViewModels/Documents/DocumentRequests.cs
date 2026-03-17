using SA.Operations.Domain.Enums;

namespace SA.Operations.Api.ViewModels.Documents;

public sealed record ChangeDocumentStatusRequest(
    DocumentStatus Status);
