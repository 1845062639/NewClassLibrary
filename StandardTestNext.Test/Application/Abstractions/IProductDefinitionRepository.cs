using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Abstractions;

public interface IProductDefinitionRepository
{
    Task<ProductDefinition?> FindByKindAsync(string productKind, CancellationToken cancellationToken = default);
    Task SaveAsync(ProductDefinition product, CancellationToken cancellationToken = default);
}
