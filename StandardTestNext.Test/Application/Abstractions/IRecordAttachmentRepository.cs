using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Abstractions;

public interface IRecordAttachmentRepository
{
    Task SaveForRecordAsync(Guid recordId, IReadOnlyCollection<RecordAttachment> attachments, CancellationToken cancellationToken = default);
    Task SaveForRecordItemAsync(Guid recordItemId, IReadOnlyCollection<RecordAttachment> attachments, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecordAttachment>> ListForRecordAsync(Guid recordId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecordAttachment>> ListForRecordItemAsync(Guid recordItemId, CancellationToken cancellationToken = default);
}
