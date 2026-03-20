using System.Text.Json;
using StandardTestNext.Contracts;
using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestProductDefinitionService : ITestProductDefinitionService
{
    private readonly IProductDefinitionRepository _productRepository;

    public TestProductDefinitionService(IProductDefinitionRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDefinition> GetOrCreateAsync(MotorRatedParamsContract ratedParams, CancellationToken cancellationToken = default)
    {
        var existing = await _productRepository.FindByKindAsync(ratedParams.ProductKind, cancellationToken);
        var latestRatedParamsJson = JsonSerializer.Serialize(ratedParams);

        if (existing is not null)
        {
            var changed = !string.Equals(existing.RatedParamsJson, latestRatedParamsJson, StringComparison.Ordinal)
                || !string.Equals(existing.Model, ratedParams.Model, StringComparison.Ordinal)
                || !string.Equals(existing.Code, ratedParams.StandardCode, StringComparison.Ordinal);

            if (!changed)
            {
                return existing;
            }

            existing.Model = ratedParams.Model;
            existing.Code = ratedParams.StandardCode;
            existing.RatedParamsJson = latestRatedParamsJson;
            existing.Remark = "Updated from latest rated params contract snapshot.";
            await _productRepository.SaveAsync(existing, cancellationToken);
            return existing;
        }

        var created = new ProductDefinition
        {
            ProductKind = ratedParams.ProductKind,
            Code = ratedParams.StandardCode,
            Model = ratedParams.Model,
            Manufacturer = "TBD",
            RatedParamsJson = latestRatedParamsJson,
            Remark = "Created from rated params contract snapshot."
        };

        await _productRepository.SaveAsync(created, cancellationToken);
        return created;
    }
}
