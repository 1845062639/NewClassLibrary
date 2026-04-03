using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Abstractions;

public interface ITestProductDefinitionService
{
    Task<ProductDefinition> GetOrCreateAsync(MotorRatedParamsContract ratedParams, CancellationToken cancellationToken = default);
}
