namespace StandardTestNext.Contracts;

public static class TestRecordQueryGatewayFactory
{
    public static ITestRecordQueryGateway Create(Func<ITestRecordQueryGateway?>? resolver = null)
    {
        var resolvedGateway = resolver?.Invoke();
        return resolvedGateway ?? new NullTestRecordQueryGateway();
    }

    private sealed class NullTestRecordQueryGateway : ITestRecordQueryGateway
    {
        public Task<IReadOnlyList<TestRecordListItemContract>> ListRecentAsync(int take = 10, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<TestRecordListItemContract> items =
            [
                new TestRecordListItemContract
                {
                    RecordCode = "PENDING-BRIDGE",
                    ProductKind = "Motor_Y",
                    ProductDisplayName = "Motor_Y / bridge-pending",
                    TestKindCode = "Routine",
                    TestTime = DateTimeOffset.Now,
                    ItemCount = 0,
                    SampleCount = 0,
                    KeyPointSampleCount = 0,
                    ContinuousSampleCount = 0,
                    RecordAttachmentCount = 0,
                    ItemAttachmentBucketCount = 0,
                    ReportCount = 0,
                    HasReportArtifacts = false,
                    ReusedProductDefinition = false,
                    ItemPartitions =
                    [
                        new TestRecordItemPartitionContract
                        {
                            ItemCode = "RealtimeKeyPoints",
                            DisplayName = "Realtime Key Points",
                            SortOrder = 100,
                            MethodCode = "MotorRealtimeSampling",
                            RecordMode = "KeyPointOnly",
                            SampleCount = 0
                        },
                        new TestRecordItemPartitionContract
                        {
                            ItemCode = "RealtimeContinuous",
                            DisplayName = "Realtime Continuous Samples",
                            SortOrder = 200,
                            MethodCode = "MotorRealtimeSampling",
                            RecordMode = "Continuous",
                            SampleCount = 0
                        }
                    ]
                }
            ];

            return Task.FromResult(items);
        }

        public Task<TestRecordDetailContract?> GetDetailAsync(string recordCode, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.Now;

            TestRecordDetailContract detail = new()
            {
                RecordCode = recordCode,
                ProductKind = "Motor_Y",
                ProductDisplayName = "Motor_Y / bridge-pending",
                TestKindCode = "Routine",
                TestTime = now,
                RecordAttachmentCount = 0,
                ItemAttachmentBucketCount = 0,
                ItemCount = 2,
                SampleCount = 18,
                KeyPointSampleCount = 6,
                ContinuousSampleCount = 12,
                HasReports = true,
                HasReportArtifacts = true,
                PrimaryReportFormat = "json",
                PrimaryReportArtifactFileName = $"{recordCode}.json",
                LightweightReportFormat = "manifest",
                LightweightReportArtifactFileName = $"{recordCode}.manifest.json",
                ItemDetails =
                [
                    new TestRecordItemDetailContract
                    {
                        ItemCode = "RealtimeKeyPoints",
                        DisplayName = "Realtime Key Points",
                        SortOrder = 100,
                        MethodCode = "MotorRealtimeSampling",
                        RecordMode = "KeyPointOnly",
                        SampleCount = 6,
                        AttachmentCount = 0,
                        IsValid = true,
                        HasRemark = true,
                        Remark = "Key-point samples extracted from realtime stream."
                    },
                    new TestRecordItemDetailContract
                    {
                        ItemCode = "RealtimeContinuous",
                        DisplayName = "Realtime Continuous Samples",
                        SortOrder = 200,
                        MethodCode = "MotorRealtimeSampling",
                        RecordMode = "Continuous",
                        SampleCount = 12,
                        AttachmentCount = 0,
                        IsValid = true,
                        HasRemark = true,
                        Remark = "Continuous samples kept as raw JSON in phase-1."
                    }
                ],
                ReportSummaries =
                [
                    new TestReportSummaryContract
                    {
                        RecordCode = recordCode,
                        Format = "json",
                        ExportedAt = now,
                        ContentLength = 2048,
                        ArtifactFileName = $"{recordCode}.json",
                        ArtifactSavedPath = $"artifacts/reports/{recordCode}.json",
                        IsLightweightEntry = false,
                        IsPrimaryEntry = true
                    },
                    new TestReportSummaryContract
                    {
                        RecordCode = recordCode,
                        Format = "manifest",
                        ExportedAt = now,
                        ContentLength = 512,
                        ArtifactFileName = $"{recordCode}.manifest.json",
                        ArtifactSavedPath = $"artifacts/reports/{recordCode}.manifest.json",
                        IsLightweightEntry = true,
                        IsPrimaryEntry = false
                    }
                ]
            };

            return Task.FromResult<TestRecordDetailContract?>(detail);
        }
    }
}
