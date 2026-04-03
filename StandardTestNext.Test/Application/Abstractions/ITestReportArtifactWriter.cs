using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Application.Abstractions;

public interface ITestReportArtifactWriter
{
    Task<TestReportArtifactDescriptor> WriteAsync(string recordCode, string format, string content, CancellationToken cancellationToken = default);
}
