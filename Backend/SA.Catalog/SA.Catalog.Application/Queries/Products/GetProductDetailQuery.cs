using Mediator;
using SA.Catalog.Application.Dtos;

namespace SA.Catalog.Application.Queries.Products;

public readonly record struct GetProductDetailQuery(Guid TenantId, Guid Id) : IQuery<ProductDetailDto>;
