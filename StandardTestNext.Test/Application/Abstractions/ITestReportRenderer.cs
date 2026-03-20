using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Application.Abstractions;

public interface ITestReportRenderer
{
    string Format { get; }
    string Render(TestReportDocument document);
}
