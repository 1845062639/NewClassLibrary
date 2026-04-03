using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class TestRecordBuildResult
{
    public required TestRecordAggregate Record { get; init; }
    public required TestRecordItemMappingResult Mapping { get; init; }
    public required TestRecordStatistics Statistics { get; init; }
}
