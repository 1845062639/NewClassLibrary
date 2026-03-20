using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Abstractions;

public interface IProductDefinitionQueryService
{
    Task<ProductDefinition?> GetByKindAsync(string productKind, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDefinition>> ListRecentAsync(int take = 20, CancellationToken cancellationToken = default);
}
