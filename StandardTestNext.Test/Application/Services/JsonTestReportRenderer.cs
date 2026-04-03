using System.Text.Json;
using StandardTestNext.Test.Application.Abstractions;

namespace StandardTestNext.Test.Application.Services;

public sealed class JsonTestReportRenderer : ITestReportRenderer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public string Format => "json";

    public string Render(TestReportDocument document)
    {
        return JsonSerializer.Serialize(document, SerializerOptions);
    }
}
