using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Application.Services;
using System.Text;

namespace StandardTestNext.Test.Infrastructure.Persistence;

public sealed class FileSystemTestReportArtifactWriter : ITestReportArtifactWriter
{
    private readonly string _baseDirectory;

    public FileSystemTestReportArtifactWriter(string? baseDirectory = null)
    {
        _baseDirectory = baseDirectory ?? Path.Combine(AppContext.BaseDirectory, "artifacts", "reports");
    }

    public async Task<TestReportArtifactDescriptor> WriteAsync(string recordCode, string format, string content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_baseDirectory);

        var safeRecordCode = BuildSafeSegment(recordCode, "record");
        var extension = BuildExtension(format);
        var timestamp = DateTimeOffset.Now;
        var fileName = $"{safeRecordCode}-{timestamp:yyyyMMdd-HHmmss}.{extension}";
        var filePath = Path.Combine(_baseDirectory, fileName);

        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8, cancellationToken);
        return new TestReportArtifactDescriptor(recordCode, extension, fileName, filePath, timestamp);
    }

    private static string BuildSafeSegment(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var safe = new string(value
            .Trim()
            .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
            .ToArray());

        return string.IsNullOrWhiteSpace(safe) ? fallback : safe;
    }

    private static string BuildExtension(string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return "txt";
        }

        return format.Trim().TrimStart('.').ToLowerInvariant();
    }
}
