using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Infrastructure.Persistence;

public sealed class InMemoryRecordAttachmentRepository : IRecordAttachmentRepository
{
    private readonly Dictionary<Guid, List<RecordAttachment>> _recordAttachments = new();
    private readonly Dictionary<Guid, List<RecordAttachment>> _recordItemAttachments = new();

    public Task SaveForRecordAsync(Guid recordId, IReadOnlyCollection<RecordAttachment> attachments, CancellationToken cancellationToken = default)
    {
        _recordAttachments[recordId] = attachments.ToList();
        return Task.CompletedTask;
    }

    public Task SaveForRecordItemAsync(Guid recordItemId, IReadOnlyCollection<RecordAttachment> attachments, CancellationToken cancellationToken = default)
    {
        _recordItemAttachments[recordItemId] = attachments.ToList();
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<RecordAttachment>> ListForRecordAsync(Guid recordId, CancellationToken cancellationToken = default)
    {
        _recordAttachments.TryGetValue(recordId, out var attachments);
        IReadOnlyList<RecordAttachment> result = attachments?.ToArray() ?? Array.Empty<RecordAttachment>();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<RecordAttachment>> ListForRecordItemAsync(Guid recordItemId, CancellationToken cancellationToken = default)
    {
        _recordItemAttachments.TryGetValue(recordItemId, out var attachments);
        IReadOnlyList<RecordAttachment> result = attachments?.ToArray() ?? Array.Empty<RecordAttachment>();
        return Task.FromResult(result);
    }
}
