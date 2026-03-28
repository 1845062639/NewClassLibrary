using StandardTestNext.Contracts;
using StandardTestNext.Test.Application.AppSide;
using StandardTestNext.Test.Application.Services;
using StandardTestNext.Test.Domain.Records;
using StandardTestNext.Test.Infrastructure.Persistence;

namespace StandardTestNext.Test.Application.Services;

public static class TestRecordQueryGatewayAdapterSmokeTests
{
    public static void Run()
    {
        ShouldExposeLegacyPayloadSummaryThroughAppQueryGateway();
        ShouldPreserveReportSelectionMetadataThroughAppQueryGateway();
        ShouldReturnNullDetailForUnknownRecordCode();
        ShouldExposeMotorYTrialPayloadShapesThroughAppQueryGateway();
        ShouldExposeMotorYMethodDecisionSummaryThroughAppQueryGateway();
    }

    private static void ShouldExposeLegacyPayloadSummaryThroughAppQueryGateway()
    {
        var now = DateTimeOffset.Parse("2026-03-23T08:00:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = now,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = "Noise",
                    MethodCode = "M-Noise",
                    IsValid = true,
                    DataJson = """
                    {
                      "SampleCount": 3,
                      "LegacySampleCount": 2,
                      "RecordMode": "legacy-replay",
                      "LegacySamples": [
                        {
                          "LeaveFactoryModePowerCurveImage": "power-a.png",
                          "LeaveFactoryModeTempCurveImage": "temp-a.png",
                          "LeaveFactoryModeVibrationCurveImage": "vibration-a.png",
                          "UabIncoming": 380.5,
                          "Temp1": 88.2
                        },
                        {
                          "TempRiseModePowerCurveImage": "power-b.png",
                          "TempRiseModeTempCurveImage": "temp-b.png",
                          "TempRiseModeVibrationFrequencyCurveImage": "vibration-b.png"
                        }
                      ]
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);

        var list = gateway.ListRecentAsync(5).GetAwaiter().GetResult();
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult();

        var listItem = list.Single();
        var partition = listItem.ItemPartitions.Single();
        if (partition.ItemCode != "Noise"
            || partition.SampleCount != 3
            || partition.LegacySampleCount != 2
            || !partition.HasLegacyPayload)
        {
            throw new InvalidOperationException("TestRecordQueryGatewayAdapter list smoke test failed.");
        }

        if (detail is null)
        {
            throw new InvalidOperationException("TestRecordQueryGatewayAdapter detail smoke test returned null.");
        }

        var item = detail.ItemDetails.Single();
        if (item.ItemCode != "Noise"
            || item.SampleCount != 3
            || item.LegacySampleCount != 2
            || !item.HasLegacyPayload
            || item.LegacyPayload.PowerCurveImageCount != 2
            || item.LegacyPayload.TempCurveImageCount != 2
            || item.LegacyPayload.VibrationCurveImageCount != 2
            || !item.LegacyPayload.HasIncomingPowerMetrics
            || !item.LegacyPayload.HasWindingTemperatureMetrics)
        {
            throw new InvalidOperationException("TestRecordQueryGatewayAdapter detail smoke test failed.");
        }

        var summary = TestRecordLegacyPayloadFormatter.FormatDetailSummary(detail.ItemDetails);
        const string expected = "Noise:legacy=2:power=2:temp=2:vibration=2:incoming=Y:winding=Y";
        if (!string.Equals(summary, expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"TestRecordQueryGatewayAdapter summary smoke test failed. Expected '{expected}', got '{summary}'.");
        }
    }

    private static void ShouldPreserveReportSelectionMetadataThroughAppQueryGateway()
    {
        var now = DateTimeOffset.Parse("2026-03-23T09:00:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-REPORT-001",
            ProductKind = "Motor_T",
            TestKindCode = "Type",
            TestTime = now,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = "TempRise",
                    MethodCode = "M-Temp",
                    IsValid = true,
                    DataJson = """
                    {
                      "SampleCount": 2,
                      "RecordMode": "key-point"
                    }
                    """
                }
            }
        };

        var reports = new[]
        {
            new TestReportSnapshot
            {
                RecordCode = record.RecordCode,
                Format = "markdown",
                Content = "# primary markdown",
                SavedAt = now.AddMinutes(5),
                ArtifactFileName = "REC-SMOKE-REPORT-001.md",
                ArtifactSavedPath = "/tmp/REC-SMOKE-REPORT-001.md",
                IsPrimaryEntry = true,
                IsLightweightEntry = false
            },
            new TestReportSnapshot
            {
                RecordCode = record.RecordCode,
                Format = "json",
                Content = "{\"recordCode\":\"REC-SMOKE-REPORT-001\"}",
                SavedAt = now.AddMinutes(3),
                ArtifactFileName = "REC-SMOKE-REPORT-001.json",
                ArtifactSavedPath = "/tmp/REC-SMOKE-REPORT-001.json",
                IsPrimaryEntry = false,
                IsLightweightEntry = true
            }
        };

        var gateway = CreateGateway(new[] { record }, reports);
        var listItem = gateway.ListRecentAsync(5).GetAwaiter().GetResult().Single();
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult();

        if (listItem.ReportCount != 2
            || !listItem.HasReportArtifacts
            || listItem.PrimaryReportFormat != "markdown"
            || listItem.PrimaryReportArtifactFileName != "REC-SMOKE-REPORT-001.md"
            || listItem.LightweightReportFormat != "json"
            || listItem.LightweightReportArtifactFileName != "REC-SMOKE-REPORT-001.json")
        {
            throw new InvalidOperationException("TestRecordQueryGatewayAdapter list report metadata smoke test failed.");
        }

        if (detail is null
            || !detail.HasReports
            || !detail.HasReportArtifacts
            || detail.PrimaryReportFormat != "markdown"
            || detail.PrimaryReportArtifactFileName != "REC-SMOKE-REPORT-001.md"
            || detail.LightweightReportFormat != "json"
            || detail.LightweightReportArtifactFileName != "REC-SMOKE-REPORT-001.json"
            || detail.ReportSummaries.Count != 2
            || !detail.ReportSummaries.Any(summary => summary.IsPrimaryEntry && summary.Format == "markdown")
            || !detail.ReportSummaries.Any(summary => summary.IsLightweightEntry && summary.Format == "json"))
        {
            throw new InvalidOperationException("TestRecordQueryGatewayAdapter detail report metadata smoke test failed.");
        }
    }

    private static void ShouldReturnNullDetailForUnknownRecordCode()
    {
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-UNKNOWN-BASE",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = DateTimeOffset.Parse("2026-03-23T10:00:00+08:00")
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync("REC-NOT-FOUND").GetAwaiter().GetResult();
        if (detail is not null)
        {
            throw new InvalidOperationException("TestRecordQueryGatewayAdapter should return null for unknown record code.");
        }
    }

    private static void ShouldExposeMotorYTrialPayloadShapesThroughAppQueryGateway()
    {
        var builder = new MotorYTrialRecordBuilder();
        var rated = new MotorRatedParamsContract
        {
            ProductKind = "Motor_Y",
            Model = "Y2-315M-4",
            RatedPower = 132,
            RatedCurrent = 240,
            RatedVoltage = 380,
            RatedSpeed = 1480,
            RatedFrequency = 50,
            Pole = 4,
            PolePairs = 2,
            Connection = "Δ"
        };

        var baseTime = DateTimeOffset.Parse("2026-03-28T10:30:00+08:00");
        var samples = new[]
        {
            new MotorRealtimeSampleContract
            {
                SampleTime = baseTime,
                ProductKind = "Motor_Y",
                VoltageAverage = 381.2,
                CurrentAverage = 52.6,
                Power = 24.8,
                Frequency = 50,
                Speed = 1492,
                Torque = 118.2,
                IsRecordPoint = true
            },
            new MotorRealtimeSampleContract
            {
                SampleTime = baseTime.AddSeconds(10),
                ProductKind = "Motor_Y",
                VoltageAverage = 379.8,
                CurrentAverage = 66.4,
                Power = 31.2,
                Frequency = 50,
                Speed = 1476,
                Torque = 136.5,
                IsRecordPoint = false
            },
            new MotorRealtimeSampleContract
            {
                SampleTime = baseTime.AddSeconds(20),
                ProductKind = "Motor_Y",
                VoltageAverage = 382.5,
                CurrentAverage = 74.1,
                Power = 36.7,
                Frequency = 50,
                Speed = 1468,
                Torque = 149.3,
                IsRecordPoint = true
            }
        };

        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = baseTime
        };

        foreach (var item in builder.BuildTrialItems(rated, samples))
        {
            record.Items.Add(item);
        }

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult();
        if (detail is null)
        {
            throw new InvalidOperationException("Motor_Y trial payload query smoke test returned null detail.");
        }

        var items = detail.ItemDetails.ToDictionary(x => x.ItemCode, StringComparer.Ordinal);
        AssertMotorYQueryItem(items, MotorYTestMethodCodes.DcResistance, 1, string.Empty);
        AssertMotorYQueryItem(items, MotorYTestMethodCodes.NoLoad, 3, TestRecordSampleModes.KeyPointOnly);
        AssertMotorYQueryItem(items, MotorYTestMethodCodes.HeatRun, 3, TestRecordSampleModes.Continuous);
        AssertMotorYQueryItem(items, MotorYTestMethodCodes.LoadA, 2, TestRecordSampleModes.Continuous);
        AssertMotorYQueryItem(items, MotorYTestMethodCodes.LoadB, 1, TestRecordSampleModes.Continuous);
        AssertMotorYQueryItem(items, MotorYTestMethodCodes.LockedRotor, 2, TestRecordSampleModes.KeyPointOnly);
    }

    private static void AssertMotorYQueryItem(
        IReadOnlyDictionary<string, TestRecordItemDetailContract> items,
        string itemCode,
        int expectedSampleCount,
        string expectedRecordMode)
    {
        if (!items.TryGetValue(itemCode, out var item))
        {
            throw new InvalidOperationException($"Motor_Y query smoke test missing item '{itemCode}'.");
        }

        if (item.SampleCount != expectedSampleCount || !string.Equals(item.RecordMode, expectedRecordMode, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Motor_Y query smoke test failed for '{itemCode}'. Expected sampleCount={expectedSampleCount}, recordMode={expectedRecordMode}; got sampleCount={item.SampleCount}, recordMode={item.RecordMode}.");
        }

        var expectedProfile = MotorYTrialItemProfileCatalog.GetRequiredBaseline(itemCode);
        if (item.BuildProfile is null
            || item.BuildProfile.MethodValue != expectedProfile.MethodValue
            || !string.Equals(item.BuildProfile.MethodKey, expectedProfile.MethodKey, StringComparison.Ordinal)
            || !string.Equals(item.BuildProfile.ProfileKey, expectedProfile.ProfileKey, StringComparison.Ordinal)
            || !string.Equals(item.BuildProfile.VariantKind, expectedProfile.VariantKind, StringComparison.Ordinal)
            || !string.Equals(item.BuildProfile.AlgorithmFamily, expectedProfile.AlgorithmFamily, StringComparison.Ordinal)
            || !string.Equals(item.BuildProfile.LegacyEnumName, expectedProfile.LegacyEnumName, StringComparison.Ordinal)
            || !string.Equals(item.BuildProfile.LegacyFormName, expectedProfile.LegacyFormName, StringComparison.Ordinal)
            || !string.Equals(item.BuildProfile.LegacyAlgorithmEntry, expectedProfile.LegacyAlgorithmEntry, StringComparison.Ordinal)
            || !string.Equals(item.BuildProfile.LegacyMethodName, expectedProfile.LegacyMethodName, StringComparison.Ordinal)
            || !string.Equals(item.BuildProfile.LegacySettingsMethodName, expectedProfile.LegacySettingsMethodName, StringComparison.Ordinal)
            || !item.BuildProfile.IsBaselineMethod)
        {
            throw new InvalidOperationException($"Motor_Y query smoke test build profile mismatch for '{itemCode}'.");
        }
    }

    private static void ShouldExposeMotorYMethodDecisionSummaryThroughAppQueryGateway()
    {
        var baseTime = DateTimeOffset.Parse("2026-03-28T11:00:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-DECISION-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = baseTime,
            Items =
            {
                CreateMotorYDecisionItem(MotorYTestMethodCodes.DcResistance, 1, baseTime),
                CreateMotorYDecisionItem(MotorYTestMethodCodes.NoLoad, 59, baseTime.AddMinutes(1)),
                CreateMotorYDecisionItem(MotorYTestMethodCodes.NoLoad, 59, baseTime.AddMinutes(2)),
                CreateMotorYDecisionItem(MotorYTestMethodCodes.NoLoad, 0, baseTime.AddMinutes(3)),
                CreateMotorYDecisionItem(MotorYTestMethodCodes.LoadA, 4, baseTime.AddMinutes(4))
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult();
        if (detail is null)
        {
            throw new InvalidOperationException("Motor_Y method decision query smoke test returned null detail.");
        }

        var decisions = detail.MotorYMethodDecisions.ToDictionary(x => x.CanonicalCode, StringComparer.Ordinal);
        AssertMethodDecision(decisions, MotorYTestMethodCodes.DcResistance, 1, 1, 1, 1, 1, false, 1d);
        AssertMethodDecision(decisions, MotorYTestMethodCodes.NoLoad, 3, 0, 1, 59, 2, true, 0.6667d);
        AssertMethodDecision(decisions, MotorYTestMethodCodes.LoadA, 1, 4, 1, 4, 1, false, 1d);
    }

    private static TestRecordItemAggregate CreateMotorYDecisionItem(string canonicalCode, int methodValue, DateTimeOffset sampleTime)
    {
        var route = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, methodValue)
            ?? throw new InvalidOperationException($"Missing Motor_Y route for {canonicalCode}:{methodValue}.");

        return new TestRecordItemAggregate
        {
            TestRecordItemId = Guid.NewGuid(),
            ItemCode = canonicalCode,
            MethodCode = route.MethodKey,
            IsValid = true,
            DataJson = $$"""
            {
              "SampleCount": 1,
              "RecordMode": "key-point",
              "BuildProfile": {
                "CanonicalCode": "{{route.CanonicalCode}}",
                "MethodValue": {{route.MethodValue}},
                "MethodKey": "{{route.MethodKey}}",
                "ProfileKey": "{{route.ProfileKey}}",
                "VariantKind": "{{route.VariantKind}}",
                "AlgorithmFamily": "{{route.AlgorithmFamily}}",
                "LegacyEnumName": "{{route.LegacyEnumName}}",
                "LegacyFormName": "{{route.LegacyFormName}}",
                "LegacyAlgorithmEntry": "{{route.LegacyAlgorithmEntry}}",
                "LegacyMethodName": "{{route.LegacyMethodName}}",
                "LegacySettingsMethodName": "{{route.LegacySettingsMethodName}}",
                "IsBaselineMethod": {{route.IsBaselineMethod.ToString().ToLowerInvariant()}}
              },
              "Samples": [
                {
                  "SampleTime": "{{sampleTime:O}}",
                  "CurrentAverage": 12.3,
                  "VoltageAverage": 380.0
                }
              ]
            }
            """
        };
    }

    private static void AssertMethodDecision(
        IReadOnlyDictionary<string, MotorYMethodDecisionContract> decisions,
        string canonicalCode,
        int expectedTotalCount,
        int expectedBaselineMethod,
        int expectedBaselineCount,
        int expectedDominantMethod,
        int expectedDominantCount,
        bool expectedPrioritize,
        double expectedDominantShare)
    {
        if (!decisions.TryGetValue(canonicalCode, out var decision))
        {
            throw new InvalidOperationException($"Motor_Y method decision query smoke test missing decision '{canonicalCode}'.");
        }

        if (decision.TotalCount != expectedTotalCount
            || decision.BaselineCount != expectedBaselineCount
            || decision.DominantCount != expectedDominantCount
            || decision.ShouldPrioritizeDominantOverBaseline != expectedPrioritize
            || Math.Abs(decision.DominantShare - expectedDominantShare) > 0.0001d)
        {
            throw new InvalidOperationException($"Motor_Y method decision query smoke test numeric mismatch for '{canonicalCode}'.");
        }

        if (decision.BaselineProfile is null
            || decision.BaselineProfile.MethodValue != expectedBaselineMethod
            || !decision.BaselineProfile.IsBaselineMethod)
        {
            throw new InvalidOperationException($"Motor_Y method decision query smoke test baseline mismatch for '{canonicalCode}'.");
        }

        if (decision.DominantProfile is null
            || decision.DominantProfile.MethodValue != expectedDominantMethod)
        {
            throw new InvalidOperationException($"Motor_Y method decision query smoke test dominant mismatch for '{canonicalCode}'.");
        }
    }


    private static TestRecordQueryGatewayAdapter CreateGateway(params TestRecordAggregate[] records)
    {
        return CreateGateway(records, Array.Empty<TestReportSnapshot>());
    }

    private static TestRecordQueryGatewayAdapter CreateGateway(
        IReadOnlyList<TestRecordAggregate> records,
        IReadOnlyList<TestReportSnapshot> reports)
    {
        var recordRepository = new InMemoryTestRecordRepository();
        var attachmentRepository = new InMemoryRecordAttachmentRepository();
        var reportRepository = new InMemoryTestReportRepository();

        foreach (var record in records)
        {
            recordRepository.SaveAsync(record).GetAwaiter().GetResult();
        }

        foreach (var report in reports)
        {
            var document = new TestReportDocument
            {
                RecordCode = report.RecordCode
            };

            reportRepository.SaveAsync(document, report.Format, report.Content).GetAwaiter().GetResult();
            reportRepository.SaveSummaryAsync(new TestReportPersistenceSummary
            {
                RecordCode = report.RecordCode,
                Format = report.Format,
                ExportedAt = report.SavedAt,
                ContentLength = report.Content.Length,
                ArtifactFileName = report.ArtifactFileName,
                ArtifactSavedPath = report.ArtifactSavedPath,
                IsLightweightEntry = report.IsLightweightEntry,
                IsPrimaryEntry = report.IsPrimaryEntry
            }).GetAwaiter().GetResult();
        }

        var queryService = new TestRecordQueryService(recordRepository, attachmentRepository, reportRepository);
        var facade = new TestRecordQueryFacade(queryService);
        return new TestRecordQueryGatewayAdapter(facade);
    }
}
