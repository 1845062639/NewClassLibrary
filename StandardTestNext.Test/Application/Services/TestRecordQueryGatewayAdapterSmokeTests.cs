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
        ShouldPreferCanonicalLegacyCodeAliasForAppMethodAdaptationPlan();
        ShouldExposeMotorYMethodDecisionSummaryThroughAppQueryGateway();
        ShouldExposeMotorYMethodAdaptationPlanThroughAppQueryGateway();
        ShouldExposeLegacyAlgorithmSourceEvidenceThroughAppQueryGateway();
        ShouldExposeAdditionalLegacyAlgorithmSourceEvidenceThroughAppQueryGateway();
        ShouldExposeDcResistanceDecisionAnchorSuggestionsThroughAppQueryGateway();
        ShouldExposeDecisionAnchorPrimaryNextFieldThroughAppQueryGateway();
        ShouldExposeDecisionAnchorPrimaryFieldDistributionsThroughAppQueryGateway();
        ShouldExposeCrossPlanDecisionAnchorPrimaryFieldFocusesThroughAppQueryGateway();
        ShouldExposeAlgorithmFamilyDecisionAnchorPrimaryFieldFocusesThroughAppQueryGateway();
        ShouldExposeRequiredResultPrimaryFieldDistributionsThroughAppQueryGateway();
        ShouldExposeCrossPlanRequiredResultPrimaryFieldFocusesThroughAppQueryGateway();
        ShouldExposeAlgorithmFamilyRequiredResultPrimaryFieldFocusesThroughAppQueryGateway();
        ShouldExposeVariantKindDecisionAnchorPrimaryFieldFocusesThroughAppQueryGateway();
        ShouldExposeVariantKindRequiredResultPrimaryFieldFocusesThroughAppQueryGateway();
        ShouldExposeHeatRunAndLoadADecisionAnchorSuggestionsThroughAppQueryGateway();
        ShouldExposeLoadBDecisionAnchorSuggestionsThroughAppQueryGateway();
        ShouldExposeLockedRotorDecisionAnchorSuggestionsThroughAppQueryGateway();
        ShouldExposeLegacyAlgorithmRoutePerMotorYItemWithoutBuildProfile();
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

    private static void ShouldPreferCanonicalLegacyCodeAliasForAppMethodAdaptationPlan()
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

        var baseTime = DateTimeOffset.Parse("2026-03-29T09:30:00+08:00");
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
            RecordCode = "REC-SMOKE-MOTORY-ALIAS-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = baseTime
        };

        foreach (var item in builder.BuildTrialItems(rated, samples))
        {
            record.Items.Add(item);
        }

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y app alias preference smoke test returned null detail.");

        var loadBPlan = detail.MotorYMethodAdaptationPlans.FirstOrDefault(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal))
            ?? throw new InvalidOperationException("Motor_Y app alias preference smoke test missing Load_B adaptation plan.");

        if (!string.Equals(loadBPlan.RecommendedLegacyCode, "B法负载试验", StringComparison.Ordinal)
            || loadBPlan.RecommendedLegacyCodeCount != 1
            || Math.Abs(loadBPlan.RecommendedLegacyCodeShare - 1d) > 0.0001d)
        {
            throw new InvalidOperationException($"Motor_Y app alias preference smoke test failed: expected canonical alias B法负载试验/1/1, actual={loadBPlan.RecommendedLegacyCode}/{loadBPlan.RecommendedLegacyCodeCount}/{loadBPlan.RecommendedLegacyCodeShare}.");
        }

        if (loadBPlan.LegacyCodeDistributions.Count != 1)
        {
            throw new InvalidOperationException($"Motor_Y app alias preference smoke test failed: expected one legacy-code distribution, actual={loadBPlan.LegacyCodeDistributions.Count}.");
        }

        var distribution = loadBPlan.LegacyCodeDistributions.FirstOrDefault(x => string.Equals(x.LegacyCode, "B法负载试验", StringComparison.Ordinal));
        if (distribution is null
            || distribution.Count != 1
            || Math.Abs(distribution.Share - 1d) > 0.0001d)
        {
            throw new InvalidOperationException($"Motor_Y app alias preference smoke test failed: legacy-code distribution mismatch. actual={(distribution is null ? "<missing>" : $"{distribution.LegacyCode}/{distribution.Count}/{distribution.Share}")}.");
        }

        if (!string.Equals(loadBPlan.LegacyCodeSelectionSummary, "recommended legacy code 'B法负载试验' for MotorY.LoadB (1/1, 100pp); legacy code variants stable for MotorY.LoadB: only 'B法负载试验' observed", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y app alias preference smoke test failed: unexpected selection summary '{loadBPlan.LegacyCodeSelectionSummary}'.");
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
        AssertMethodDecision(decisions, MotorYTestMethodCodes.DcResistance, 1, 1, 1, 1, 1, false, 1d, 1d, 0, 0,
            "kept baseline method 1 because baseline already matches dominant distribution (100.00%)");
        AssertMethodDecision(decisions, MotorYTestMethodCodes.NoLoad, 3, 0, 1, 59, 2, false, 0.6667d, 0.3333d, 1, 33,
            "kept baseline method 0 because dominant method 59 share 66.67% did not reach threshold 70%");
        AssertMethodDecision(decisions, MotorYTestMethodCodes.LoadA, 1, 4, 1, 4, 1, false, 1d, 1d, 0, 0,
            "kept baseline method 4 because baseline already matches dominant distribution (100.00%)");

        AssertDistribution(decisions, MotorYTestMethodCodes.DcResistance, 1, 1, 1d, true);
        AssertDistribution(decisions, MotorYTestMethodCodes.NoLoad, 59, 2, 0.6667d, false);
        AssertDistribution(decisions, MotorYTestMethodCodes.NoLoad, 0, 1, 0.3333d, true);
        AssertDistribution(decisions, MotorYTestMethodCodes.LoadA, 4, 1, 1d, true);
        AssertDistributionOrdering(decisions, MotorYTestMethodCodes.NoLoad, 59, 0);
    }

    private static void ShouldExposeMotorYMethodAdaptationPlanThroughAppQueryGateway()
    {
        var baseTime = DateTimeOffset.Parse("2026-03-28T11:30:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-PLAN-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = baseTime,
            Items =
            {
                CreateMotorYDetailedDecisionItem(MotorYTestMethodCodes.NoLoad, 59, baseTime),
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
            throw new InvalidOperationException("Motor_Y method adaptation plan query smoke test returned null detail.");
        }

        var plans = detail.MotorYMethodAdaptationPlans.ToDictionary(x => x.CanonicalCode, StringComparer.Ordinal);
        AssertMethodAdaptationPlan(plans, MotorYTestMethodCodes.NoLoad, 4, 0, 1, 59, 3, 59, 3, true, 0.75d, "dominant-threshold-over-baseline");
        AssertMethodAdaptationPlan(plans, MotorYTestMethodCodes.LoadA, 1, 4, 1, 4, 1, 4, 1, false, 1d, "baseline");

        var noLoadPlan = plans[MotorYTestMethodCodes.NoLoad];
        var loadAPlan = plans[MotorYTestMethodCodes.LoadA];
        if (noLoadPlan.ObservedUpstreamCanonicalCodeCount != 0
            || noLoadPlan.ObservedUpstreamCanonicalCodes.Count != 0
            || noLoadPlan.ObservedUpstreamLegacyCodes.Count != 0
            || !noLoadPlan.MissingUpstreamCanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.DcResistance }, StringComparer.Ordinal)
            || !string.Equals(noLoadPlan.UpstreamDependencySummary, "upstream dependencies missing 1/1: MotorY.DcResistance; observed 0/1 required upstream codes; no legacy upstream aliases observed", StringComparison.Ordinal)
            || noLoadPlan.UpstreamLegacyCodeDistributions.Count != 2
            || noLoadPlan.UpstreamLegacyCodeDistributions.Any(x => !string.Equals(x.CanonicalCode, MotorYTestMethodCodes.DcResistance, StringComparison.Ordinal))
            || !noLoadPlan.UpstreamLegacyCodeDistributions.Select(x => x.LegacyCode).SequenceEqual(new[] { "直流电阻测定", "陪试直流电阻测定" }, StringComparer.Ordinal)
            || noLoadPlan.UpstreamLegacyCodeDistributions.Any(x => x.Count != 0)
            || noLoadPlan.UpstreamLegacyCodeDistributions.Any(x => Math.Abs(x.Share) > 0.0001d))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test upstream observation mismatch for '{MotorYTestMethodCodes.NoLoad}'. observedCount={noLoadPlan.ObservedUpstreamCanonicalCodeCount}, observed=[{string.Join(", ", noLoadPlan.ObservedUpstreamCanonicalCodes)}], observedLegacy={noLoadPlan.ObservedUpstreamLegacyCodes.Count}, missing=[{string.Join(", ", noLoadPlan.MissingUpstreamCanonicalCodes)}], upstreamDistributions={string.Join(" | ", noLoadPlan.UpstreamLegacyCodeDistributions.Select(x => $"{x.CanonicalCode}:{x.LegacyCode}:{x.Count}:{x.Share:0.####}"))}, summary='{noLoadPlan.UpstreamDependencySummary}'");
        }

        if (noLoadPlan.MissingFormulaSignalCount != 0
            || !noLoadPlan.CoveredFormulaSignals.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[]
            {
                "formula-signal:CoefficientOfPfe",
                "formula-signal:Pfe",
                "formula-signal:Pfw",
                "formula-signal:R0",
                "formula-signal:θ0"
            }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || noLoadPlan.MissingFormulaSignals.Count != 0
            || Math.Abs(noLoadPlan.FormulaSignalCoverageRatio - 1d) > 0.0001d
            || noLoadPlan.FormulaSignalCoveragePercentagePoints != 100
            || noLoadPlan.MissingLegacyAlgorithmRuleCount != 0
            || !noLoadPlan.CoveredLegacyAlgorithmRules.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[]
            {
                "legacy-rule:CoefficientOfPfe",
                "legacy-rule:I0",
                "legacy-rule:P0",
                "legacy-rule:Pcu",
                "legacy-rule:Pfe",
                "legacy-rule:Pfw",
                "legacy-rule:RConverseType",
                "legacy-rule:ΔI0"
            }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || noLoadPlan.MissingLegacyAlgorithmRules.Count != 0
            || Math.Abs(noLoadPlan.LegacyAlgorithmRuleCoverageRatio - 1d) > 0.0001d
            || noLoadPlan.LegacyAlgorithmRuleCoveragePercentagePoints != 100)
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test formula/rule coverage mismatch for '{MotorYTestMethodCodes.NoLoad}'. actual formulaCovered=[{string.Join(", ", noLoadPlan.CoveredFormulaSignals)}], rulesCovered=[{string.Join(", ", noLoadPlan.CoveredLegacyAlgorithmRules)}]");
        }

        if (noLoadPlan.CoveredRequiredResultFieldCount != 7
            || noLoadPlan.MissingRequiredResultFieldCount != 0
            || !noLoadPlan.CoveredRequiredResultFields.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] { "CoefficientOfPfe", "I0", "P0", "Pcu", "Pfe", "Pfw", "ΔI0" }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || noLoadPlan.MissingRequiredResultFields.Count != 0
            || Math.Abs(noLoadPlan.RequiredResultFieldCoverageRatio - 1d) > 0.0001d
            || noLoadPlan.RequiredResultFieldCoveragePercentagePoints != 100
            || !string.Equals(noLoadPlan.RequiredResultFieldCoverageSummary, "result required fields covered 7/7 (100pp); missing: none", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test result-field coverage mismatch for '{MotorYTestMethodCodes.NoLoad}'. covered=[{string.Join(", ", noLoadPlan.CoveredRequiredResultFields)}], missing=[{string.Join(", ", noLoadPlan.MissingRequiredResultFields)}], summary='{noLoadPlan.RequiredResultFieldCoverageSummary}'");
        }

        if (!noLoadPlan.RequiredIntermediateResultFields.SequenceEqual(new[] { "R0", "θ0", "Pcon", "P0cu1", "Pfw", "Pfe", "CoefficientOfPfe" }, StringComparer.Ordinal)
            || noLoadPlan.CoveredRequiredIntermediateResultFieldCount != 7
            || noLoadPlan.MissingRequiredIntermediateResultFieldCount != 0
            || !noLoadPlan.CoveredRequiredIntermediateResultFields.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] { "CoefficientOfPfe", "P0cu1", "Pcon", "Pfe", "Pfw", "R0", "θ0" }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || noLoadPlan.MissingRequiredIntermediateResultFields.Count != 0
            || Math.Abs(noLoadPlan.RequiredIntermediateResultFieldCoverageRatio - 1d) > 0.0001d
            || noLoadPlan.RequiredIntermediateResultFieldCoveragePercentagePoints != 100
            || !string.Equals(noLoadPlan.RequiredIntermediateResultFieldCoverageSummary, "result required fields covered 7/7 (100pp); missing: none", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test intermediate-result coverage mismatch for '{MotorYTestMethodCodes.NoLoad}'. required=[{string.Join(", ", noLoadPlan.RequiredIntermediateResultFields)}], covered=[{string.Join(", ", noLoadPlan.CoveredRequiredIntermediateResultFields)}], missing=[{string.Join(", ", noLoadPlan.MissingRequiredIntermediateResultFields)}], summary='{noLoadPlan.RequiredIntermediateResultFieldCoverageSummary}'");
        }

        if (!string.Equals(noLoadPlan.SelectedMethodSummary, "selected 空载试验 method 59 (delivery) covering 3/4 items (75.00%)", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test selected summary mismatch for '{MotorYTestMethodCodes.NoLoad}'. actual='{noLoadPlan.SelectedMethodSummary}'");
        }

        if (!string.Equals(noLoadPlan.RecommendedLegacyCode, "空载特性完全试验", StringComparison.Ordinal)
            || !string.Equals(noLoadPlan.DominantLegacyCode, "空载特性完全试验", StringComparison.Ordinal)
            || noLoadPlan.RecommendedLegacyCodeCount != 4
            || Math.Abs(noLoadPlan.RecommendedLegacyCodeShare - 0.1667d) > 0.0001d
            || !string.Equals(noLoadPlan.LegacyCodeSelectionSummary, "recommended legacy code '空载特性完全试验' for MotorY.NoLoad (4/24, 17pp); legacy code variants for MotorY.NoLoad: '空载特性完全试验' leads runner-up '空载特性测量' by 0 rows (0pp) across 6 aliases", StringComparison.Ordinal)
            || noLoadPlan.LegacyCodeDistributions.Count != 6
            || !string.Equals(noLoadPlan.LegacyCodeDistributions[0].LegacyCode, "空载特性完全试验", StringComparison.Ordinal)
            || noLoadPlan.LegacyCodeDistributions[0].Count != 4
            || Math.Abs(noLoadPlan.LegacyCodeDistributions[0].Share - 0.1667d) > 0.0001d
            || !string.Equals(noLoadPlan.LegacyCodeDistributions[5].LegacyCode, "陪试空载特性试验", StringComparison.Ordinal)
            || noLoadPlan.LegacyCodeDistributions[5].Count != 4
            || Math.Abs(noLoadPlan.LegacyCodeDistributions[5].Share - 0.1667d) > 0.0001d)
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test legacy-code distribution mismatch for '{MotorYTestMethodCodes.NoLoad}'. recommended={noLoadPlan.RecommendedLegacyCode}:{noLoadPlan.RecommendedLegacyCodeCount}:{noLoadPlan.RecommendedLegacyCodeShare:0.####}, distributions={string.Join(" | ", noLoadPlan.LegacyCodeDistributions.Select(x => $"{x.LegacyCode}:{x.Count}:{x.Share:0.####}"))}, summary='{noLoadPlan.LegacyCodeSelectionSummary}'");
        }

        if (noLoadPlan.ObservedAlgorithmInputFieldCount < 25
            || !noLoadPlan.ObservedAlgorithmInputFields.Contains("DataList", StringComparer.Ordinal)
            || !noLoadPlan.ObservedAlgorithmInputFields.Contains("DataList.I0", StringComparer.Ordinal)
            || !noLoadPlan.ObservedAlgorithmInputFields.Contains("Pfw", StringComparer.Ordinal)
            || !noLoadPlan.ObservedAlgorithmInputFields.Contains("R0", StringComparer.Ordinal)
            || !noLoadPlan.ObservedAlgorithmInputFields.Contains("R1c", StringComparer.Ordinal)
            || !noLoadPlan.ObservedAlgorithmInputFields.Contains("θ0", StringComparer.Ordinal)
            || !noLoadPlan.ObservedAlgorithmInputFields.Contains("θ1c", StringComparer.Ordinal)
            || !noLoadPlan.MissingAlgorithmInputFields.SequenceEqual(new[] { MotorYTestMethodCodes.DcResistance }, StringComparer.Ordinal)
            || noLoadPlan.AlgorithmInputFieldCoveragePercentagePoints < 95
            || noLoadPlan.LegacyAlgorithmInputsReady
            || !noLoadPlan.LegacyDecisionAnchorReady)
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test algorithm-input summary mismatch for '{MotorYTestMethodCodes.NoLoad}'. observed=[{string.Join(", ", noLoadPlan.ObservedAlgorithmInputFields)}], missingCount={noLoadPlan.MissingAlgorithmInputFieldCount}, ready={noLoadPlan.LegacyAlgorithmInputsReady}, decisionReady={noLoadPlan.LegacyDecisionAnchorReady}, readiness='{noLoadPlan.LegacyAlgorithmInputReadinessSummary}', summary='{noLoadPlan.AlgorithmInputFieldCoverageSummary}'");
        }

        if (!loadAPlan.RequiredRawDataSignals.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] { "Frequency", "I1", "Nt", "P1t", "Tt", "U" }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || loadAPlan.ObservedRawDataSignals.Count != 0
            || loadAPlan.MissingRawDataSignals.Count != 6
            || loadAPlan.RawDataSignalCoveredCount != 0
            || loadAPlan.RawDataSignalMissingCount != 6
            || loadAPlan.RawDataSampleCount != 0
            || loadAPlan.RawDataListAvailable
            || loadAPlan.RawDataSignalCoverageRatio != 0d
            || loadAPlan.RawDataSignalCoveragePercentagePoints != 0
            || !string.Equals(loadAPlan.RawDataSignalCoverageSummary, "raw data signals covered 0/6 (0pp); raw samples=0; missing: U, I1, P1t, Nt, Tt, Frequency; observed: none", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test raw-data signal coverage mismatch for '{MotorYTestMethodCodes.LoadA}'. summary='{loadAPlan.RawDataSignalCoverageSummary}'");
        }

        if (!noLoadPlan.RequiredStructuredPayloadSignals.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] { "DataList.I0", "DataList.P0", "DataList.P0cu1", "DataList.Pcon", "DataList.Pfe", "DataList.T0", "DataList.U0", "DataList.n0" }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || !noLoadPlan.ObservedStructuredPayloadSignals.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] { "DataList.I0", "DataList.P0", "DataList.P0cu1", "DataList.Pcon", "DataList.Pfe", "DataList.T0", "DataList.U0", "DataList.n0" }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || noLoadPlan.MissingStructuredPayloadSignals.Count != 0
            || noLoadPlan.StructuredPayloadSignalCoveredCount != 8
            || noLoadPlan.StructuredPayloadSignalMissingCount != 0
            || noLoadPlan.StructuredPayloadSampleCount != 1
            || !noLoadPlan.StructuredPayloadAvailable
            || Math.Abs(noLoadPlan.StructuredPayloadSignalCoverageRatio - 1d) > 0.0001d
            || noLoadPlan.StructuredPayloadSignalCoveragePercentagePoints != 100
            || noLoadPlan.MinimumStructuredPayloadSampleCount != 3
            || noLoadPlan.StructuredPayloadSampleCountReady
            || noLoadPlan.StructuredPayloadSampleCountGap != 2
            || !string.Equals(noLoadPlan.StructuredPayloadSampleCountReadinessSummary, "structured payload sample count insufficient 1/3", StringComparison.Ordinal)
            || !string.Equals(noLoadPlan.StructuredPayloadSampleCountDecisionSummary, "structured payload sample count gate blocked for MotorY.NoLoad: observed 1, still need 2 more samples to reach 3", StringComparison.Ordinal)
            || !noLoadPlan.StructuredPayloadSignalCoverageSummary.StartsWith("structured payload signals covered 8/8 (100pp); samples=1; missing: none; observed:", StringComparison.Ordinal)
            || !noLoadPlan.StructuredPayloadSignalCoverageSummary.Contains("DataList.U0", StringComparison.Ordinal)
            || !noLoadPlan.StructuredPayloadSignalCoverageSummary.Contains("DataList.T0", StringComparison.Ordinal)
            || !noLoadPlan.RequiredStructuredResultSignals.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] { "CoefficientOfPfe", "I0", "P0", "Pcu", "Pfe", "Pfw", "ΔI0" }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || !noLoadPlan.ObservedStructuredResultSignals.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] { "CoefficientOfPfe", "I0", "P0", "Pcu", "Pfe", "Pfw", "ΔI0" }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || noLoadPlan.MissingStructuredResultSignals.Count != 0
            || noLoadPlan.StructuredResultSignalCoveredCount != 7
            || noLoadPlan.StructuredResultSignalMissingCount != 0
            || noLoadPlan.StructuredResultSampleCount != 1
            || !noLoadPlan.StructuredResultAvailable
            || Math.Abs(noLoadPlan.StructuredResultSignalCoverageRatio - 1d) > 0.0001d
            || noLoadPlan.StructuredResultSignalCoveragePercentagePoints != 100
            || noLoadPlan.MinimumStructuredResultSampleCount != 1
            || !noLoadPlan.StructuredResultSampleCountReady
            || noLoadPlan.StructuredResultSampleCountGap != 0
            || !string.Equals(noLoadPlan.StructuredResultSampleCountReadinessSummary, "structured result sample count ready 1/1", StringComparison.Ordinal)
            || !string.Equals(noLoadPlan.StructuredResultSampleCountDecisionSummary, "structured result sample count gate passed for MotorY.NoLoad: observed 1 >= required 1", StringComparison.Ordinal)
            || !noLoadPlan.StructuredResultSignalCoverageSummary.StartsWith("structured result signals covered 7/7 (100pp); samples=1; missing: none; observed:", StringComparison.Ordinal)
            || !noLoadPlan.StructuredResultSignalCoverageSummary.Contains("CoefficientOfPfe", StringComparison.Ordinal)
            || !noLoadPlan.StructuredResultSignalCoverageSummary.Contains("ΔI0", StringComparison.Ordinal)
            || noLoadPlan.MinimumRawSampleCount != 0
            || !noLoadPlan.RawSampleCountReady
            || noLoadPlan.RawSampleCountGap != 0
            || !string.Equals(noLoadPlan.RawSampleCountReadinessSummary, "raw sample count requirement not set; observed 0", StringComparison.Ordinal)
            || !string.Equals(noLoadPlan.RawSampleCountDecisionSummary, "raw sample count gate disabled for MotorY.NoLoad; observed 0", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test structured/sample-count coverage mismatch for '{MotorYTestMethodCodes.NoLoad}'. payload='{noLoadPlan.StructuredPayloadSignalCoverageSummary}', payloadDecision='{noLoadPlan.StructuredPayloadSampleCountDecisionSummary}', result='{noLoadPlan.StructuredResultSignalCoverageSummary}', resultDecision='{noLoadPlan.StructuredResultSampleCountDecisionSummary}', rawDecision='{noLoadPlan.RawSampleCountDecisionSummary}'");
        }

        if (!string.Equals(noLoadPlan.BaselineDominantComparisonSummary, "baseline 0 (baseline)=1/4 (25.00%), dominant 59 (delivery)=3/4 (75.00%)", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test comparison summary mismatch for '{MotorYTestMethodCodes.NoLoad}'. actual='{noLoadPlan.BaselineDominantComparisonSummary}'");
        }

        var noLoadBuckets = noLoadPlan.DependencyBuckets.ToDictionary(x => x.BucketKey, StringComparer.Ordinal);
        if (noLoadBuckets.Count != 13
            || !noLoadBuckets.TryGetValue("formula-signals", out var noLoadFormulaBucket)
            || noLoadFormulaBucket.RequiredCount != 5
            || noLoadFormulaBucket.CoveredCount != 5
            || noLoadFormulaBucket.MissingCount != 0
            || !string.Equals(noLoadFormulaBucket.Summary, "formula signals covered 5/5 (100pp); missing: none", StringComparison.Ordinal)
            || !noLoadBuckets.TryGetValue("legacy-rules", out var noLoadRuleBucket)
            || noLoadRuleBucket.RequiredCount != 8
            || noLoadRuleBucket.CoveredCount != 8
            || noLoadRuleBucket.MissingCount != 0
            || !string.Equals(noLoadRuleBucket.Summary, "legacy algorithm rules covered 8/8 (100pp); missing: none", StringComparison.Ordinal)
            || !noLoadBuckets.TryGetValue("intermediate-result-fields", out var noLoadIntermediateBucket)
            || noLoadIntermediateBucket.RequiredCount != 7
            || noLoadIntermediateBucket.CoveredCount != 7
            || noLoadIntermediateBucket.MissingCount != 0
            || !string.Equals(noLoadIntermediateBucket.Summary, "result required fields covered 7/7 (100pp); missing: none", StringComparison.Ordinal)
            || !noLoadBuckets.TryGetValue("legacy-decision-anchor-resolutions", out var noLoadDecisionResolutionBucket)
            || noLoadDecisionResolutionBucket.RequiredCount != 3
            || noLoadDecisionResolutionBucket.CoveredCount != 3
            || noLoadDecisionResolutionBucket.MissingCount != 0
            || !string.Equals(noLoadDecisionResolutionBucket.Summary, "decision anchor resolutions covered 3/3 (100pp); partial=0; missing=0; unresolved: none", StringComparison.Ordinal)
            || !noLoadBuckets.TryGetValue("legacy-decision-anchor-fields", out var noLoadDecisionFieldBucket)
            || noLoadDecisionFieldBucket.RequiredCount != 8
            || noLoadDecisionFieldBucket.CoveredCount != 8
            || noLoadDecisionFieldBucket.MissingCount != 0
            || !string.Equals(noLoadDecisionFieldBucket.Summary, "decision anchor required fields covered 8/8 (100pp); missing: none", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test intermediate-result bucket mismatch for '{MotorYTestMethodCodes.NoLoad}'. bucketCount={noLoadBuckets.Count}, summary='{(noLoadBuckets.TryGetValue("intermediate-result-fields", out var bucket) ? bucket.Summary : "<missing>")}', decisionFieldSummary='{(noLoadBuckets.TryGetValue("legacy-decision-anchor-fields", out var fieldBucket) ? fieldBucket.Summary : "<missing>")}'");
        }

        if (noLoadPlan.LegacyDecisionAnchors.Count != 3
            || noLoadPlan.LegacyDecisionAnchorObservationRules.Count != 3
            || noLoadPlan.LegacyDecisionAnchorResolutions.Count != 3)
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test decision-anchor summary mismatch for '{MotorYTestMethodCodes.NoLoad}'. anchors='{noLoadPlan.LegacyDecisionAnchorSummary}', observed='{noLoadPlan.LegacyDecisionAnchorsObservedPayloadSummary}', rules='{noLoadPlan.LegacyDecisionAnchorObservationRuleSummary}', next='{noLoadPlan.LegacyDecisionAnchorNextActionSummary}', suggested='{noLoadPlan.SuggestedDecisionAnchorNextStepSummary}'");
        }

        var noLoadAnchorRuleMap = noLoadPlan.LegacyDecisionAnchorObservationRules.ToDictionary(x => x.AnchorKey, StringComparer.Ordinal);
        if (!noLoadAnchorRuleMap.TryGetValue("rconverse-branch", out var rconverseRule)
            || !rconverseRule.RequiredPayloadFields.SequenceEqual(new[] { "RConverseType" }, StringComparer.Ordinal)
            || !rconverseRule.ObservedPayloadFields.SequenceEqual(new[] { "RConverseType" }, StringComparer.Ordinal)
            || rconverseRule.MissingPayloadFields.Count != 0
            || !rconverseRule.CoveredByObservedPayload
            || !string.Equals(rconverseRule.Summary, "decision-anchor-observation:rconverse-branch covered by observed payload fields 'RConverseType'", StringComparison.Ordinal)
            || !noLoadAnchorRuleMap.TryGetValue("pfw-fit-window", out var pfwRule)
            || !pfwRule.RequiredPayloadFields.SequenceEqual(new[] { "Pfw" }, StringComparer.Ordinal)
            || !pfwRule.ObservedPayloadFields.SequenceEqual(new[] { "Pfw" }, StringComparer.Ordinal)
            || pfwRule.MissingPayloadFields.Count != 0
            || !pfwRule.CoveredByObservedPayload
            || !string.Equals(pfwRule.Summary, "decision-anchor-observation:pfw-fit-window covered by observed payload fields 'Pfw'", StringComparison.Ordinal)
            || !noLoadAnchorRuleMap.TryGetValue("rated-regression-ready", out var regressionRule)
            || !regressionRule.RequiredPayloadFields.SequenceEqual(new[] { "CoefficientOfPfe", "I0", "ΔI0", "P0", "Pcu", "Pfe" }, StringComparer.Ordinal)
            || !regressionRule.ObservedPayloadFields.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(new[] { "CoefficientOfPfe", "I0", "ΔI0", "P0", "Pcu", "Pfe" }.OrderBy(x => x, StringComparer.Ordinal), StringComparer.Ordinal)
            || regressionRule.MissingPayloadFields.Count != 0
            || !regressionRule.CoveredByObservedPayload
            || !string.Equals(regressionRule.Summary, "decision-anchor-observation:rated-regression-ready covered by observed payload fields 'CoefficientOfPfe', 'I0', 'P0', 'Pcu', 'Pfe', 'ΔI0'", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test decision-anchor observation rule mismatch for '{MotorYTestMethodCodes.NoLoad}'. rules={string.Join(" | ", noLoadPlan.LegacyDecisionAnchorObservationRules.Select(x => $"{x.AnchorKey}:{x.Summary}"))}");
        }

        if (Math.Abs(noLoadPlan.BaselineShare - 0.25d) > 0.0001d
            || Math.Abs(noLoadPlan.DominantShare - 0.75d) > 0.0001d
            || Math.Abs(noLoadPlan.SelectedShare - 0.75d) > 0.0001d
            || Math.Abs(noLoadPlan.SelectedLeadCountVsBaseline - 2d) > 0.0001d
            || noLoadPlan.SelectedLeadPercentagePointsVsBaseline != 50)
        {
            throw new InvalidOperationException(
                $"Motor_Y method adaptation plan query smoke test share summary mismatch for '{MotorYTestMethodCodes.NoLoad}'. baselineShare={noLoadPlan.BaselineShare}, dominantShare={noLoadPlan.DominantShare}, selectedShare={noLoadPlan.SelectedShare}, leadCount={noLoadPlan.SelectedLeadCountVsBaseline}, leadPp={noLoadPlan.SelectedLeadPercentagePointsVsBaseline}");
        }

        if (!string.Equals(noLoadPlan.LegacyCodeSelectionSummary, "recommended legacy code '空载特性完全试验' for MotorY.NoLoad (4/24, 17pp); legacy code variants for MotorY.NoLoad: '空载特性完全试验' leads runner-up '空载特性测量' by 0 rows (0pp) across 6 aliases", StringComparison.Ordinal)
            || !string.Equals(noLoadPlan.RecommendedLegacyCode, "空载特性完全试验", StringComparison.Ordinal)
            || !string.Equals(noLoadPlan.DominantLegacyCode, "空载特性完全试验", StringComparison.Ordinal)
            || noLoadPlan.RecommendedLegacyCodeCount != 4
            || Math.Abs(noLoadPlan.RecommendedLegacyCodeShare - 0.1667d) > 0.0001d
            || noLoadPlan.LegacyCodeDistributions.Count != 6
            || !string.Equals(noLoadPlan.LegacyCodeDistributions[0].CanonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal)
            || !string.Equals(noLoadPlan.LegacyCodeDistributions[0].LegacyCode, "空载特性完全试验", StringComparison.Ordinal)
            || noLoadPlan.LegacyCodeDistributions[0].Count != 4
            || Math.Abs(noLoadPlan.LegacyCodeDistributions[0].Share - 0.1667d) > 0.0001d)
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test legacy-code summary mismatch for '{MotorYTestMethodCodes.NoLoad}'. summary='{noLoadPlan.LegacyCodeSelectionSummary}', recommended='{noLoadPlan.RecommendedLegacyCode}', count={noLoadPlan.RecommendedLegacyCodeCount}, share={noLoadPlan.RecommendedLegacyCodeShare}, distributions={noLoadPlan.LegacyCodeDistributions.Count}");
        }
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

    private static TestRecordItemAggregate CreateMotorYDetailedDecisionItem(string canonicalCode, int methodValue, DateTimeOffset sampleTime)
    {
        var route = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, methodValue)
            ?? throw new InvalidOperationException($"Missing Motor_Y route for {canonicalCode}:{methodValue}.");

        string dataJson;
        if (string.Equals(canonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal))
        {
            dataJson = $$"""
            {
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
              "Un": 380.0,
              "R1c": 12.5,
              "θ1c": 26.5,
              "K1": 235.0,
              "Order": 3,
              "DataList": [
                {
                  "U0": 380.0,
                  "I0": 12.3,
                  "P0": 3.4,
                  "P0cu1": 1.2,
                  "Pcon": 2.1,
                  "Pfe": 0.9,
                  "n0": 1498.0,
                  "T0": 2.4
                }
              ],
              "P0": 3.4,
              "I0": 12.3,
              "ΔI0": 0.4,
              "Pcu": 1.2,
              "Pfw": 0.8,
              "Pfe": 0.9,
              "θ0": 27.6,
              "R0": 12.55,
              "CoefficientOfPfe": [0.0, 0.12, 0.03],
              "RConverseType": 0
            }
            """;
        }
        else if (string.Equals(canonicalCode, MotorYTestMethodCodes.HeatRun, StringComparison.Ordinal))
        {
            dataJson = $$"""
            {
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
              "GB": 1,
              "Pn": 55.0,
              "HotStateType": 1,
              "Rn": 1.42,
              "θs": 95.0,
              "θw": 102.5,
              "Data1List": [
                { "Time": 0, "P1": 100.0, "θ1": 80.0, "θb": 29.5 },
                { "Time": 30, "P1": 101.0, "θ1": 81.2, "θb": 30.1 },
                { "Time": 60, "P1": 102.5, "θ1": 82.4, "θb": 30.6 }
              ],
              "Data2List": [
                { "Time": 0, "R": 1.18 },
                { "Time": 30, "R": 1.23 },
                { "Time": 60, "R": 1.29 }
              ]
            }
            """;
        }
        else if (string.Equals(canonicalCode, MotorYTestMethodCodes.LoadA, StringComparison.Ordinal))
        {
            dataJson = $$"""
            {
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
              "CoefficientOfPfe": 0.97,
              "Pfw": 120.0,
              "θa": 75.0,
              "RawDataList": [
                { "U": 380.0, "I1": 10.2, "P1t": 500.0, "Nt": 1450.0, "Tt": 12.0, "Frequency": 50.0, "θ1t": 85.0 },
                { "U": 380.0, "I1": 11.6, "P1t": 560.0, "Nt": 1438.0, "Tt": 13.8, "Frequency": 50.0, "θ1t": 86.2 }
              ],
              "ResultDataList": [
                { "Rate": 0.5, "Pcu1": 55.0, "Pcu2": 72.0, "η": 0.88 },
                { "Rate": 1.0, "Pcu1": 82.0, "Pcu2": 110.0, "η": 0.91 }
              ],
              "Pcu1": 82.0,
              "Pcu2": 110.0,
              "η": 0.91
            }
            """;
        }
        else if (string.Equals(canonicalCode, MotorYTestMethodCodes.LockedRotor, StringComparison.Ordinal))
        {
            dataJson = $$"""
            {
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
              "Method": {{route.MethodValue}},
              "RatioOfUkToUn": 0.82,
              "RawDataList": [
                { "Uk": 115.0, "Ik": 185.0, "Pk": 12.6, "Tk": 78.0 },
                { "Uk": 118.0, "Ik": 188.0, "Pk": 12.9, "Tk": 79.5 }
              ],
              "Ikn": 186.5,
              "Pkn": 12.75,
              "Tkn": 78.75
            }
            """;
        }
        else
        {
            return CreateMotorYDecisionItem(canonicalCode, methodValue, sampleTime);
        }

        return new TestRecordItemAggregate
        {
            TestRecordItemId = Guid.NewGuid(),
            ItemCode = canonicalCode,
            MethodCode = route.MethodKey,
            IsValid = true,
            DataJson = dataJson
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
        double expectedDominantShare,
        double expectedBaselineShare,
        int expectedDominantLeadCount,
        int expectedDominantLeadPercentagePoints,
        string expectedRecommendationReason)
    {
        if (!decisions.TryGetValue(canonicalCode, out var decision))
        {
            throw new InvalidOperationException($"Motor_Y method decision query smoke test missing decision '{canonicalCode}'.");
        }

        if (decision.TotalCount != expectedTotalCount
            || decision.BaselineCount != expectedBaselineCount
            || decision.DominantCount != expectedDominantCount
            || decision.ShouldPrioritizeDominantOverBaseline != expectedPrioritize
            || Math.Abs(decision.DominantShare - expectedDominantShare) > 0.0001d
            || Math.Abs(decision.BaselineShare - expectedBaselineShare) > 0.0001d
            || decision.DominantLeadCount != expectedDominantLeadCount
            || decision.DominantLeadPercentagePoints != expectedDominantLeadPercentagePoints
            || !string.Equals(decision.RecommendationReason, expectedRecommendationReason, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Motor_Y method decision query smoke test numeric mismatch for '{canonicalCode}'. actual: total={decision.TotalCount}, baselineCount={decision.BaselineCount}, dominantCount={decision.DominantCount}, prioritize={decision.ShouldPrioritizeDominantOverBaseline}, dominantShare={decision.DominantShare}, baselineShare={decision.BaselineShare}, leadCount={decision.DominantLeadCount}, leadPp={decision.DominantLeadPercentagePoints}, reason='{decision.RecommendationReason}'");
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

        var expectedRecommendedMethod = expectedPrioritize
            ? expectedDominantMethod
            : expectedBaselineMethod;
        var expectedRecommendedStrategy = expectedPrioritize
            ? "dominant-threshold-over-baseline"
            : "baseline";
        if (decision.RecommendedProfile is null
            || decision.RecommendedProfile.MethodValue != expectedRecommendedMethod
            || !string.Equals(decision.RecommendedStrategy, expectedRecommendedStrategy, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method decision query smoke test recommended route mismatch for '{canonicalCode}'.");
        }
    }
    private static void AssertDistribution(
        IReadOnlyDictionary<string, MotorYMethodDecisionContract> decisions,
        string canonicalCode,
        int expectedMethod,
        int expectedCount,
        double expectedShare,
        bool expectedBaseline)
    {
        if (!decisions.TryGetValue(canonicalCode, out var decision))
        {
            throw new InvalidOperationException($"Motor_Y method decision query smoke test missing decision '{canonicalCode}' for distribution assertion.");
        }

        var distribution = decision.Distributions.FirstOrDefault(x => x.MethodValue == expectedMethod);
        if (distribution is null)
        {
            throw new InvalidOperationException($"Motor_Y method decision query smoke test missing distribution '{canonicalCode}:{expectedMethod}'.");
        }

        if (distribution.Count != expectedCount
            || Math.Abs(distribution.Share - expectedShare) > 0.0001d)
        {
            throw new InvalidOperationException($"Motor_Y method decision query smoke test distribution numeric mismatch for '{canonicalCode}:{expectedMethod}'.");
        }

        if (distribution.Profile is null
            || distribution.Profile.MethodValue != expectedMethod
            || distribution.Profile.IsBaselineMethod != expectedBaseline)
        {
            throw new InvalidOperationException($"Motor_Y method decision query smoke test distribution profile mismatch for '{canonicalCode}:{expectedMethod}'.");
        }
    }

    private static void AssertDistributionOrdering(
        IReadOnlyDictionary<string, MotorYMethodDecisionContract> decisions,
        string canonicalCode,
        params int[] expectedMethodsInOrder)
    {
        if (!decisions.TryGetValue(canonicalCode, out var decision))
        {
            throw new InvalidOperationException($"Motor_Y method decision query smoke test missing decision '{canonicalCode}' for ordering assertion.");
        }

        var actual = decision.Distributions.Select(x => x.MethodValue).ToArray();
        if (!actual.SequenceEqual(expectedMethodsInOrder))
        {
            throw new InvalidOperationException(
                $"Motor_Y method decision query smoke test distribution ordering mismatch for '{canonicalCode}'. expected={string.Join(",", expectedMethodsInOrder)}, actual={string.Join(",", actual)}");
        }
    }

    private static void AssertMethodAdaptationPlan(
        IReadOnlyDictionary<string, MotorYMethodAdaptationPlanContract> plans,
        string canonicalCode,
        int expectedTotalCount,
        int expectedBaselineMethod,
        int expectedBaselineCount,
        int expectedDominantMethod,
        int expectedDominantCount,
        int expectedSelectedMethod,
        int expectedSelectedCount,
        bool expectedShouldUseDominant,
        double expectedDominantShare,
        string expectedSelectionStrategy)
    {
        if (!plans.TryGetValue(canonicalCode, out var plan))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test missing plan '{canonicalCode}'.");
        }

        if (plan.TotalCount != expectedTotalCount
            || plan.BaselineCount != expectedBaselineCount
            || plan.DominantCount != expectedDominantCount
            || plan.SelectedCount != expectedSelectedCount
            || plan.ShouldUseDominantRoute != expectedShouldUseDominant
            || Math.Abs(plan.DominantShare - expectedDominantShare) > 0.0001d
            || !string.Equals(plan.SelectionStrategy, expectedSelectionStrategy, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test numeric mismatch for '{canonicalCode}'.");
        }

        if (plan.BaselineProfile is null
            || plan.BaselineProfile.MethodValue != expectedBaselineMethod
            || !plan.BaselineProfile.IsBaselineMethod)
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test baseline mismatch for '{canonicalCode}'.");
        }

        if (plan.DominantProfile is null
            || plan.DominantProfile.MethodValue != expectedDominantMethod)
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test dominant mismatch for '{canonicalCode}'.");
        }

        if (plan.SelectedProfile is null
            || plan.SelectedProfile.MethodValue != expectedSelectedMethod)
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test selected profile mismatch for '{canonicalCode}'.");
        }

        var selectedRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, expectedSelectedMethod)
            ?? throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test missing selected route for '{canonicalCode}:{expectedSelectedMethod}'.");
        if (!string.Equals(plan.AlgorithmEntry, selectedRoute.LegacyAlgorithmEntry, StringComparison.Ordinal)
            || !string.Equals(plan.SettingsMethodName, selectedRoute.LegacySettingsMethodName, StringComparison.Ordinal)
            || !string.Equals(plan.LegacyMethodName, selectedRoute.LegacyMethodName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test selected metadata mismatch for '{canonicalCode}'.");
        }

        var expectedLeadCount = Math.Max(0, expectedDominantCount - expectedBaselineCount);
        var expectedLeadPercentagePoints = Math.Max(0, (int)Math.Round((expectedDominantShare - (expectedTotalCount <= 0 ? 0d : (double)expectedBaselineCount / expectedTotalCount)) * 100d, MidpointRounding.AwayFromZero));
        if (plan.DominantLeadCount != expectedLeadCount
            || plan.DominantLeadPercentagePoints != expectedLeadPercentagePoints)
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test lead summary mismatch for '{canonicalCode}'.");
        }

        var expectedReason = expectedShouldUseDominant
            ? $"selected dominant method {expectedDominantMethod} over baseline {expectedBaselineMethod} because dominant share {expectedDominantShare:P2} reached threshold {0.7d:P0} (+{expectedLeadCount} items, +{expectedLeadPercentagePoints}pp)"
            : expectedBaselineMethod == expectedDominantMethod
                ? $"kept baseline method {expectedBaselineMethod} because baseline already matches dominant distribution ({expectedDominantShare:P2})"
                : $"kept baseline method {expectedBaselineMethod} because dominant method {expectedDominantMethod} share {expectedDominantShare:P2} did not reach threshold {0.7d:P0}";
        if (!string.Equals(plan.SelectionReason, expectedReason, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test selection reason mismatch for '{canonicalCode}'. expected='{expectedReason}', actual='{plan.SelectionReason}'");
        }

        var expectedSelectedSummary = $"selected {selectedRoute.LegacyMethodName} method {expectedSelectedMethod} ({selectedRoute.VariantKind}) covering {expectedSelectedCount}/{expectedTotalCount} items ({(expectedTotalCount <= 0 ? 0d : Math.Round((double)expectedSelectedCount / expectedTotalCount, 4, MidpointRounding.AwayFromZero)):P2})";
        if (!string.Equals(plan.SelectedMethodSummary, expectedSelectedSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test selected summary mismatch for '{canonicalCode}'. expected='{expectedSelectedSummary}', actual='{plan.SelectedMethodSummary}'");
        }

        var expectedComparisonSummary = $"baseline {expectedBaselineMethod} ({plan.BaselineProfile?.VariantKind ?? string.Empty})={expectedBaselineCount}/{expectedTotalCount} ({(expectedTotalCount <= 0 ? 0d : Math.Round((double)expectedBaselineCount / expectedTotalCount, 4, MidpointRounding.AwayFromZero)):P2}), dominant {expectedDominantMethod} ({plan.DominantProfile?.VariantKind ?? string.Empty})={expectedDominantCount}/{expectedTotalCount} ({expectedDominantShare:P2})";
        if (!string.Equals(plan.BaselineDominantComparisonSummary, expectedComparisonSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test comparison summary mismatch for '{canonicalCode}'. expected='{expectedComparisonSummary}', actual='{plan.BaselineDominantComparisonSummary}'");
        }

        var distribution = plan.Distributions.Select(x => x.MethodValue).ToArray();
        if (canonicalCode == MotorYTestMethodCodes.NoLoad && !distribution.SequenceEqual(new[] { 59, 0 }))
        {
            throw new InvalidOperationException($"Motor_Y method adaptation plan query smoke test distribution ordering mismatch for '{canonicalCode}'. actual=[{string.Join(", ", distribution)}]");
        }
    }

    private static void ShouldExposeLegacyAlgorithmSourceEvidenceThroughAppQueryGateway()
    {
        var baseTime = DateTimeOffset.Parse("2026-03-29T09:15:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-SOURCE-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = baseTime,
            Items =
            {
                CreateMotorYDetailedDecisionItem(MotorYTestMethodCodes.NoLoad, 59, baseTime),
                CreateMotorYDetailedDecisionItem(MotorYTestMethodCodes.LoadB, 5, baseTime.AddMinutes(1))
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult();
        if (detail is null)
        {
            throw new InvalidOperationException("Motor_Y legacy algorithm source evidence query smoke test returned null detail.");
        }

        var plans = detail.MotorYMethodAdaptationPlans.ToDictionary(x => x.CanonicalCode, StringComparer.Ordinal);
        var noLoadPlan = plans[MotorYTestMethodCodes.NoLoad];
        var loadBPlan = plans[MotorYTestMethodCodes.LoadB];

        if (noLoadPlan.SourceEvidences.Count != 3
            || !noLoadPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "rconverse-branch", StringComparison.Ordinal)
                && x.StartLine == 184
                && x.EndLine == 193
                && x.ReferencedFields.Contains("RConverseType", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("R0", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("θ0", StringComparer.Ordinal))
            || !noLoadPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "per-point-precompute", StringComparison.Ordinal)
                && x.StartLine == 195
                && x.EndLine == 202
                && x.ReferencedFields.Contains("DataList.U0", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("DataList.Pcon", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("DataList.P0cu1", StringComparer.Ordinal))
            || !noLoadPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "pfw-and-pfe-fit", StringComparison.Ordinal)
                && x.StartLine == 216
                && x.EndLine == 238
                && x.ReferencedFields.Contains("CoefficientOfPfe", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("Pfw", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("Pfe", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("I0", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("ΔI0", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("P0", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("Pcu", StringComparer.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm source evidence query smoke test mismatch for '{MotorYTestMethodCodes.NoLoad}'. actual=[{string.Join(" | ", noLoadPlan.SourceEvidences.Select(x => $"{x.SectionKey}:{x.MethodName}:{x.StartLine}-{x.EndLine}:{string.Join(",", x.ReferencedFields)}"))}]");
        }

        if (loadBPlan.SourceEvidences.Count != 4
            || !loadBPlan.SourceEvidences.Select(x => x.SectionKey).SequenceEqual(new[] { "raw-point-precompute", "correlation-refit", "gb-thermal-branch", "result-ps-iteration" }, StringComparer.Ordinal)
            || !loadBPlan.SourceEvidences.Select(x => x.MethodName).All(x => string.Equals(x, "Load_B", StringComparison.Ordinal))
            || !loadBPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "correlation-refit", StringComparison.Ordinal)
                && x.StartLine == 320
                && x.EndLine == 332
                && x.ReferencedFields.SequenceEqual(new[] { "A", "B", "R", "Tx", "Pl", "bad-point-refit" }, StringComparer.Ordinal))
            || !loadBPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "result-ps-iteration", StringComparison.Ordinal)
                && x.StartLine == 398
                && x.EndLine == 454
                && x.ReferencedFields.Contains("ResultDataList", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("Ps", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("cuC", StringComparer.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm source evidence query smoke test mismatch for '{MotorYTestMethodCodes.LoadB}'. actual=[{string.Join(" | ", loadBPlan.SourceEvidences.Select(x => $"{x.SectionKey}:{x.MethodName}:{x.StartLine}-{x.EndLine}:{string.Join(",", x.ReferencedFields)}"))}]");
        }

        if (noLoadPlan.FormDependencyEvidences.Count != 1
            || !noLoadPlan.FormDependencyEvidences.Any(x => string.Equals(x.FormName, "FrmMotor_Y_NoLoad", StringComparison.Ordinal)
                && string.Equals(x.SourceFile, "ClassLibary/StandardTest/View/Motor/Y/FrmMotor_Y_NoLoad.cs", StringComparison.Ordinal)
                && x.Line == 263
                && string.Equals(x.SourceAnchor, "FrmMotor_Y_NoLoad:dc-resistance-prefill", StringComparison.Ordinal)
                && x.UpstreamCanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.DcResistance }, StringComparer.Ordinal)
                && x.ReferencedMethods.SequenceEqual(new[] { "TestRecordHelper.GetTestRecordItem<TestData_Motor_Y_Direct_Current_Resistance>" }, StringComparer.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy form dependency evidence query smoke test mismatch for '{MotorYTestMethodCodes.NoLoad}'. actual=[{string.Join(" | ", noLoadPlan.FormDependencyEvidences.Select(x => $"{x.FormName}:{x.Line}:{string.Join(",", x.UpstreamCanonicalCodes)}:{string.Join(",", x.ReferencedMethods)}"))}]");
        }

        if (loadBPlan.FormDependencyEvidences.Count != 2
            || !loadBPlan.FormDependencyEvidences.Any(x => string.Equals(x.FormName, "FrmMotor_Y_Load_B", StringComparison.Ordinal)
                && string.Equals(x.SourceFile, "ClassLibary/StandardTest/View/Motor/Y/FrmMotor_Y_Load_B.cs", StringComparison.Ordinal)
                && x.Line == 272
                && x.UpstreamCanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal)
                && x.ReferencedMethods.SequenceEqual(new[] { "TestRecordHelper.GetTestRecordItem<TestData_Motor_Y_NoLoad>" }, StringComparer.Ordinal))
            || !loadBPlan.FormDependencyEvidences.Any(x => string.Equals(x.FormName, "FrmMotor_Y_Load_B", StringComparison.Ordinal)
                && x.Line == 266
                && x.UpstreamCanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.HeatRun }, StringComparer.Ordinal)
                && x.ReferencedMethods.SequenceEqual(new[] { "TestRecordHelper.GetTestRecordItem<TestData_Motor_Y_Thermal>" }, StringComparer.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy form dependency evidence query smoke test mismatch for '{MotorYTestMethodCodes.LoadB}'. actual=[{string.Join(" | ", loadBPlan.FormDependencyEvidences.Select(x => $"{x.FormName}:{x.Line}:{string.Join(",", x.UpstreamCanonicalCodes)}:{string.Join(",", x.ReferencedMethods)}"))}]");
        }
    }

    private static void ShouldExposeAdditionalLegacyAlgorithmSourceEvidenceThroughAppQueryGateway()
    {
        var baseTime = DateTimeOffset.Parse("2026-03-29T09:30:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-SOURCE-002",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = baseTime,
            Items =
            {
                CreateMotorYDetailedDecisionItem(MotorYTestMethodCodes.HeatRun, 3, baseTime),
                CreateMotorYDetailedDecisionItem(MotorYTestMethodCodes.LoadA, 4, baseTime.AddMinutes(1)),
                CreateMotorYDetailedDecisionItem(MotorYTestMethodCodes.LockedRotor, 11, baseTime.AddMinutes(2))
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult();
        if (detail is null)
        {
            throw new InvalidOperationException("Motor_Y additional legacy algorithm source evidence query smoke test returned null detail.");
        }

        var plans = detail.MotorYMethodAdaptationPlans.ToDictionary(x => x.CanonicalCode, StringComparer.Ordinal);
        var heatRunPlan = plans[MotorYTestMethodCodes.HeatRun];
        var loadAPlan = plans[MotorYTestMethodCodes.LoadA];
        var lockedRotorPlan = plans[MotorYTestMethodCodes.LockedRotor];

        if (heatRunPlan.SourceEvidences.Count != 4
            || !heatRunPlan.SourceEvidences.Select(x => x.SectionKey).SequenceEqual(new[] { "first-seconds-interval", "theta-b-last-quarter-window", "rn-selection", "gb-temperature-branch" }, StringComparer.Ordinal)
            || !heatRunPlan.SourceEvidences.Select(x => x.MethodName).All(x => string.Equals(x, "Thermal", StringComparison.Ordinal))
            || !heatRunPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "theta-b-last-quarter-window", StringComparison.Ordinal)
                && x.StartLine == 563
                && x.EndLine == 569
                && x.ReferencedFields.Contains("Data1List.θb", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("θb", StringComparer.Ordinal))
            || !heatRunPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "gb-temperature-branch", StringComparison.Ordinal)
                && x.StartLine == 648
                && x.EndLine == 679
                && x.ReferencedFields.Contains("GB", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("θw", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("θs", StringComparer.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y additional source evidence smoke test mismatch for '{MotorYTestMethodCodes.HeatRun}'. actual=[{string.Join(" | ", heatRunPlan.SourceEvidences.Select(x => $"{x.SectionKey}:{x.MethodName}:{x.StartLine}-{x.EndLine}:{string.Join(",", x.ReferencedFields)}"))}]");
        }

        if (loadAPlan.SourceEvidences.Count != 3
            || !loadAPlan.SourceEvidences.Select(x => x.SectionKey).SequenceEqual(new[] { "raw-point-precompute", "temperature-correction", "result-fit-grid" }, StringComparer.Ordinal)
            || !loadAPlan.SourceEvidences.Select(x => x.MethodName).All(x => string.Equals(x, "Load_A", StringComparison.Ordinal))
            || !loadAPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "raw-point-precompute", StringComparison.Ordinal)
                && x.StartLine == 697
                && x.EndLine == 717
                && x.ReferencedFields.Contains("Pfw", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("Pfe", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("P2tx", StringComparer.Ordinal))
            || !loadAPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "temperature-correction", StringComparison.Ordinal)
                && x.StartLine == 718
                && x.EndLine == 730
                && x.ReferencedFields.Contains("θa", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("η", StringComparer.Ordinal))
            || !loadAPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "result-fit-grid", StringComparison.Ordinal)
                && x.StartLine == 731
                && x.EndLine == 794
                && x.ReferencedFields.Contains("ResultDataList", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("Percentage", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("η", StringComparer.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y additional source evidence smoke test mismatch for '{MotorYTestMethodCodes.LoadA}'. actual=[{string.Join(" | ", loadAPlan.SourceEvidences.Select(x => $"{x.SectionKey}:{x.MethodName}:{x.StartLine}-{x.EndLine}:{string.Join(",", x.ReferencedFields)}"))}]");
        }

        if (lockedRotorPlan.SourceEvidences.Count != 3
            || !lockedRotorPlan.SourceEvidences.Select(x => x.SectionKey).SequenceEqual(new[] { "torquecal-precompute", "low-voltage-logfit", "near-rated-fit" }, StringComparer.Ordinal)
            || !lockedRotorPlan.SourceEvidences.Select(x => x.MethodName).All(x => string.Equals(x, "Lock_Rotor", StringComparison.Ordinal))
            || !lockedRotorPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "torquecal-precompute", StringComparison.Ordinal)
                && x.StartLine == 26
                && x.EndLine == 47
                && x.ReferencedFields.Contains("TorqueCalType", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("DataList.Pfe", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("K1", StringComparer.Ordinal))
            || !lockedRotorPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "low-voltage-logfit", StringComparison.Ordinal)
                && x.StartLine == 49
                && x.EndLine == 71
                && x.ReferencedFields.Contains("Ikn", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("Tkn", StringComparer.Ordinal))
            || !lockedRotorPlan.SourceEvidences.Any(x => string.Equals(x.SectionKey, "near-rated-fit", StringComparison.Ordinal)
                && x.StartLine == 72
                && x.EndLine == 87
                && x.ReferencedFields.Contains("Pkn", StringComparer.Ordinal)
                && x.ReferencedFields.Contains("TknDivideTn", StringComparer.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y additional source evidence smoke test mismatch for '{MotorYTestMethodCodes.LockedRotor}'. actual=[{string.Join(" | ", lockedRotorPlan.SourceEvidences.Select(x => $"{x.SectionKey}:{x.MethodName}:{x.StartLine}-{x.EndLine}:{string.Join(",", x.ReferencedFields)}"))}]");
        }
    }

    private static void ShouldExposeDcResistanceDecisionAnchorSuggestionsThroughAppQueryGateway()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-29T10:00:00+08:00");
        var noDataRecord = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-DCR-DECISION-EMPTY-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                CreateMotorYDecisionItem(MotorYTestMethodCodes.DcResistance, 1, sampleTime)
            }
        };

        var noDataGateway = CreateGateway(noDataRecord);
        var noDataDetail = noDataGateway.GetDetailAsync(noDataRecord.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y DcResistance decision-anchor smoke test returned null detail for empty payload.");
        var noDataPlan = noDataDetail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.DcResistance, StringComparison.Ordinal));

        if (!string.Equals(noDataPlan.LegacyDecisionAnchorGapPreviewSummary, "decision anchor gaps: cold-baseline-ready[missing]:R1, R1c, θ1c; downstream-ready[missing]:R1, R1c, θ1c", StringComparison.Ordinal)
            || !string.Equals(noDataPlan.LegacyDecisionAnchorNextActionSummary, "decision anchor next actions: need DcResistance cold-baseline fields R1, R1c, θ1c; need DcResistance downstream-ready fields R1, R1c, θ1c", StringComparison.Ordinal)
            || !noDataPlan.SuggestedDecisionAnchorNextSteps.SequenceEqual(new[]
            {
                "先补直流电阻冷态基线结果：R1, R1c, θ1c",
                "先补直流电阻下游承接结果：R1, R1c, θ1c"
            }, StringComparer.Ordinal)
            || !string.Equals(noDataPlan.SuggestedDecisionAnchorNextStepSummary, "先补直流电阻冷态基线结果：R1, R1c, θ1c; 先补直流电阻下游承接结果：R1, R1c, θ1c", StringComparison.Ordinal)
            || noDataPlan.LegacyDecisionAnchorResolutions.Count != 2
            || noDataPlan.ResolvedLegacyDecisionAnchorCount != 0
            || noDataPlan.PartialLegacyDecisionAnchorCount != 0
            || noDataPlan.MissingLegacyDecisionAnchorResolutionCount != 2
            || !noDataPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "cold-baseline-ready", StringComparison.Ordinal)
                && !resolution.ResolvedByObservedPayload
                && !resolution.PartiallyResolvedByObservedPayload
                && string.Equals(resolution.SuggestedNextStepCategory, "baseline-result", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepFocus, "直流电阻冷态基线结果", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "R1", "R1c", "θ1c" }, StringComparer.Ordinal)
                && resolution.SuggestedNextSteps.SequenceEqual(new[] { "先补直流电阻冷态基线结果：R1, R1c, θ1c" }, StringComparer.Ordinal)
                && string.Equals(resolution.SuggestedNextStepSummary, "先补直流电阻冷态基线结果：R1, R1c, θ1c", StringComparison.Ordinal))
            || !noDataPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "downstream-ready", StringComparison.Ordinal)
                && !resolution.ResolvedByObservedPayload
                && !resolution.PartiallyResolvedByObservedPayload
                && string.Equals(resolution.SuggestedNextStepCategory, "downstream-readiness", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepFocus, "直流电阻下游承接结果", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "R1", "R1c", "θ1c" }, StringComparer.Ordinal)
                && resolution.SuggestedNextSteps.SequenceEqual(new[] { "先补直流电阻下游承接结果：R1, R1c, θ1c" }, StringComparer.Ordinal)
                && string.Equals(resolution.SuggestedNextStepSummary, "先补直流电阻下游承接结果：R1, R1c, θ1c", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y DcResistance decision-anchor query smoke test empty-payload mismatch. next='{noDataPlan.LegacyDecisionAnchorNextActionSummary}', gap='{noDataPlan.LegacyDecisionAnchorGapPreviewSummary}', suggested='{noDataPlan.SuggestedDecisionAnchorNextStepSummary}'");
        }

        var readyRecord = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-DCR-DECISION-READY-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime.AddMinutes(5),
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.DcResistance,
                    MethodCode = "DirectCurrentResistance:1",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 1,
                      "Ruv": 1.1,
                      "Rvw": 1.2,
                      "Rwu": 1.3,
                      "R1": 1.2,
                      "θ1c": 25.0
                    }
                    """
                }
            }
        };

        var readyGateway = CreateGateway(readyRecord);
        var readyDetail = readyGateway.GetDetailAsync(readyRecord.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y DcResistance decision-anchor smoke test returned null detail for ready payload.");
        var readyPlan = readyDetail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.DcResistance, StringComparison.Ordinal));

        if (!string.Equals(readyPlan.LegacyDecisionAnchorGapPreviewSummary, "decision anchor gaps: none", StringComparison.Ordinal)
            || !string.Equals(readyPlan.LegacyDecisionAnchorNextActionSummary, "decision anchors ready; no additional branch evidence required", StringComparison.Ordinal)
            || readyPlan.SuggestedDecisionAnchorNextSteps.Count != 0
            || !string.Equals(readyPlan.SuggestedDecisionAnchorNextStepSummary, "no decision-anchor next-step recommendation", StringComparison.Ordinal)
            || readyPlan.LegacyDecisionAnchorResolutions.Count != 2
            || readyPlan.ResolvedLegacyDecisionAnchorCount != 2
            || readyPlan.PartialLegacyDecisionAnchorCount != 0
            || readyPlan.MissingLegacyDecisionAnchorResolutionCount != 0
            || readyPlan.LegacyDecisionAnchorResolutionCoverageRatio != 1d
            || readyPlan.LegacyDecisionAnchorResolutionCoveragePercentagePoints != 100
            || !readyPlan.LegacyDecisionAnchorResolutions.All(resolution => resolution.ResolvedByObservedPayload
                && !resolution.PartiallyResolvedByObservedPayload
                && string.Equals(resolution.ResolutionStage, "resolved", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepSummary, "decision anchor already resolved", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y DcResistance decision-anchor query smoke test ready-payload mismatch. next='{readyPlan.LegacyDecisionAnchorNextActionSummary}', gap='{readyPlan.LegacyDecisionAnchorGapPreviewSummary}', suggested='{readyPlan.SuggestedDecisionAnchorNextStepSummary}'");
        }
    }

    private static void ShouldExposeDecisionAnchorPrimaryNextFieldThroughAppQueryGateway()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-30T00:10:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-PRIMARY-NEXT-FIELD-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.LoadB,
                    MethodCode = "LoadB:5",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 5,
                      "RawDataList": [
                        { "U": 380, "I1": 10, "P1t": 500, "Nt": 1450, "Tt": 12, "Frequency": 50, "θ1t": 85 }
                      ]
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y decision-anchor primary-next-field smoke test returned null detail.");

        var loadBPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal));
        var routeResolution = loadBPlan.LegacyDecisionAnchorResolutions.SingleOrDefault(x => string.Equals(x.AnchorKey, "gb-ratios-branch", StringComparison.Ordinal));
        var refitResolution = loadBPlan.LegacyDecisionAnchorResolutions.SingleOrDefault(x => string.Equals(x.AnchorKey, "correlation-refit", StringComparison.Ordinal));
        var psResolution = loadBPlan.LegacyDecisionAnchorResolutions.SingleOrDefault(x => string.Equals(x.AnchorKey, "ps-iteration", StringComparison.Ordinal));
        var thermalResolution = loadBPlan.LegacyDecisionAnchorResolutions.SingleOrDefault(x => string.Equals(x.AnchorKey, "thermal-carryover", StringComparison.Ordinal));

        if (routeResolution is null
            || !string.Equals(routeResolution.SuggestedPrimaryNextField, "GB", StringComparison.Ordinal)
            || !string.Equals(routeResolution.SuggestedPrimaryNextFieldSummary, "优先补字段 GB，用于推进 B法 GB/ratios/θs 分支字段（gb-ratios-branch）", StringComparison.Ordinal)
            || refitResolution is null
            || !string.Equals(refitResolution.SuggestedPrimaryNextField, "R", StringComparison.Ordinal)
            || !string.Equals(refitResolution.SuggestedPrimaryNextFieldSummary, "优先补字段 R，用于推进 B法坏点剔除后二次拟合证据（correlation-refit）", StringComparison.Ordinal)
            || psResolution is null
            || !string.Equals(psResolution.SuggestedPrimaryNextField, "Ps", StringComparison.Ordinal)
            || !string.Equals(psResolution.SuggestedPrimaryNextFieldSummary, "优先补字段 Ps，用于推进 B法 Ps 非负迭代收敛字段（ps-iteration）", StringComparison.Ordinal)
            || thermalResolution is null
            || !string.Equals(thermalResolution.SuggestedPrimaryNextField, "θw", StringComparison.Ordinal)
            || !string.Equals(thermalResolution.SuggestedPrimaryNextFieldSummary, "优先补字段 θw，用于推进 B法热态承接字段（thermal-carryover）", StringComparison.Ordinal)
            || !string.IsNullOrEmpty(loadBPlan.DecisionAnchorTopPriority)
            || !string.IsNullOrEmpty(loadBPlan.DecisionAnchorTopPriorityDominantAnchorKey)
            || !string.IsNullOrEmpty(loadBPlan.DecisionAnchorTopPriorityPrimaryField)
            || !string.IsNullOrEmpty(loadBPlan.DecisionAnchorTopPriorityPrimaryFieldSummary)
            || loadBPlan.DecisionAnchorTopPriorityDetail is not null)
        {
            throw new InvalidOperationException($"Motor_Y decision-anchor primary-next-field query smoke test mismatch. actualTop={loadBPlan.DecisionAnchorTopPriority}/{loadBPlan.DecisionAnchorTopPriorityDominantAnchorKey}/{loadBPlan.DecisionAnchorTopPriorityPrimaryField}/'{loadBPlan.DecisionAnchorTopPriorityPrimaryFieldSummary}'; actual=[{string.Join(" | ", loadBPlan.LegacyDecisionAnchorResolutions.Select(x => $"{x.AnchorKey}:{x.SuggestedPrimaryNextField}:{x.SuggestedPrimaryNextFieldSummary}"))}]");
        }
    }

    private static void ShouldExposeDecisionAnchorPrimaryFieldDistributionsThroughAppQueryGateway()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-30T08:25:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-PRIMARY-FIELD-DIST-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.LoadB,
                    MethodCode = "LoadB:5",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 5,
                      "RawDataList": [
                        { "U": 380, "I1": 10, "P1t": 500, "Nt": 1450, "Tt": 12, "Frequency": 50, "θ1t": 85 }
                      ]
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y decision-anchor primary-field distribution smoke test returned null detail.");

        var loadBPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal));
        var distributions = loadBPlan.DecisionAnchorPrimaryFieldDistributions;
        var r = distributions.SingleOrDefault(x => string.Equals(x.PrimaryField, "R", StringComparison.Ordinal));
        var gb = distributions.SingleOrDefault(x => string.Equals(x.PrimaryField, "GB", StringComparison.Ordinal));
        var ps = distributions.SingleOrDefault(x => string.Equals(x.PrimaryField, "Ps", StringComparison.Ordinal));
        var thetaW = distributions.SingleOrDefault(x => string.Equals(x.PrimaryField, "θw", StringComparison.Ordinal));

        if (distributions.Count != 4
            || r is null
            || r.Count != 1
            || !r.AnchorKeys.SequenceEqual(new[] { "correlation-refit" }, StringComparer.Ordinal)
            || !r.SuggestedNextStepFocuses.SequenceEqual(new[] { "B法坏点剔除后二次拟合证据" }, StringComparer.Ordinal)
            || !r.SuggestedNextStepPriorities.SequenceEqual(new[] { "blocking" }, StringComparer.Ordinal)
            || !r.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.LoadB }, StringComparer.Ordinal)
            || !string.Equals(r.Summary, "decision-anchor primary field R suggested by 1/4 anchors (25pp); anchors=correlation-refit; focus=B法坏点剔除后二次拟合证据; priorities=blocking", StringComparison.Ordinal)
            || gb is null
            || !gb.AnchorKeys.SequenceEqual(new[] { "gb-ratios-branch" }, StringComparer.Ordinal)
            || ps is null
            || !ps.AnchorKeys.SequenceEqual(new[] { "ps-iteration" }, StringComparer.Ordinal)
            || thetaW is null
            || !thetaW.AnchorKeys.SequenceEqual(new[] { "thermal-carryover" }, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y decision-anchor primary-field distribution query smoke test mismatch. actual=[{string.Join(" | ", distributions.Select(x => $"{x.PrimaryField}:{x.Count}:{string.Join("/", x.AnchorKeys)}:{string.Join("/", x.SuggestedNextStepFocuses)}:{string.Join("/", x.SuggestedNextStepPriorities)}"))}]");
        }
    }

    private static void ShouldExposeCrossPlanDecisionAnchorPrimaryFieldFocusesThroughAppQueryGateway()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-30T08:23:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-CROSS-PLAN-PRIMARY-FIELD-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.NoLoad,
                    MethodCode = "NoLoad:0",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 0,
                      "DataList": [
                        { "U": 190, "I": 6.3, "P": 520, "Cos": 0.81 }
                      ],
                      "Un": 380,
                      "K1": 1,
                      "R1c": 1.25,
                      "θ1c": 25
                    }
                    """
                },
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.LoadB,
                    MethodCode = "LoadB:5",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 5,
                      "RawDataList": [
                        { "U": 380, "I1": 10, "P1t": 500, "Nt": 1450, "Tt": 12, "Frequency": 50, "θ1t": 85 }
                      ]
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y cross-plan decision-anchor primary-field focus smoke test returned null detail.");

        var noLoadPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal));
        var loadBPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal));

        var noLoadCrossPlan = noLoadPlan.CrossPlanDecisionAnchorPrimaryFieldFocuses;
        var loadBCrossPlan = loadBPlan.CrossPlanDecisionAnchorPrimaryFieldFocuses;
        var rConverse = noLoadCrossPlan.SingleOrDefault(x => string.Equals(x.PrimaryField, "RConverseType", StringComparison.Ordinal));
        var pfw = noLoadCrossPlan.SingleOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));
        var gb = noLoadCrossPlan.SingleOrDefault(x => string.Equals(x.PrimaryField, "GB", StringComparison.Ordinal));

        var ps = noLoadCrossPlan.SingleOrDefault(x => string.Equals(x.PrimaryField, "Ps", StringComparison.Ordinal));

        if (noLoadCrossPlan.Count != 7
            || loadBCrossPlan.Count != noLoadCrossPlan.Count
            || !string.Equals(loadBPlan.CrossPlanDecisionAnchorPrimaryFieldSummary, noLoadPlan.CrossPlanDecisionAnchorPrimaryFieldSummary, StringComparison.Ordinal)
            || !noLoadPlan.CrossPlanDecisionAnchorPrimaryFieldSummary.Contains("cross-plan decision-anchor primary fields top 3/7", StringComparison.Ordinal)
            || !noLoadPlan.CrossPlanDecisionAnchorPrimaryFieldSummary.Contains("CoefficientOfPfe=1", StringComparison.Ordinal)
            || !noLoadPlan.CrossPlanDecisionAnchorPrimaryFieldSummary.Contains("GB=1", StringComparison.Ordinal)
            || !noLoadPlan.CrossPlanDecisionAnchorPrimaryFieldSummary.Contains("Pfw=1", StringComparison.Ordinal)
            || rConverse is null
            || pfw is null
            || gb is null
            || ps is null)
        {
            throw new InvalidOperationException($"Motor_Y cross-plan decision-anchor primary-field focus query smoke test mismatch. noLoadSummary='{noLoadPlan.CrossPlanDecisionAnchorPrimaryFieldSummary}', loadBSummary='{loadBPlan.CrossPlanDecisionAnchorPrimaryFieldSummary}', actual=[{string.Join(" | ", noLoadCrossPlan.Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:{string.Join("/", x.CanonicalCodes)}:{string.Join("/", x.AnchorKeys)}:{string.Join("/", x.SuggestedNextStepPriorities)}"))}]");
        }
    }

    private static void ShouldExposeAlgorithmFamilyDecisionAnchorPrimaryFieldFocusesThroughAppQueryGateway()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-30T08:23:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-ALGO-FAMILY-ANCHOR-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.NoLoad,
                    MethodCode = "NoLoad:0",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 0,
                      "DataList": [
                        { "U": 190, "I": 6.3, "P": 520, "Cos": 0.81 }
                      ],
                      "Un": 380,
                      "K1": 1,
                      "R1c": 1.25,
                      "θ1c": 25
                    }
                    """
                },
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.LoadB,
                    MethodCode = "LoadB:5",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 5,
                      "RawDataList": [
                        { "U": 380, "I1": 10, "P1t": 500, "Nt": 1450, "Tt": 12, "Frequency": 50, "θ1t": 85 }
                      ]
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y algorithm-family decision-anchor primary-field focus smoke test returned null detail.");

        var noLoadPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal));
        var loadBPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal));
        var focuses = noLoadPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldFocuses;
        var gb = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "GB", StringComparison.Ordinal));
        var pfw = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));
        var ps = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "Ps", StringComparison.Ordinal));
        var r = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "R", StringComparison.Ordinal));

        if (focuses.Count != 7
            || loadBPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldFocuses.Count != focuses.Count
            || !string.Equals(loadBPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary, noLoadPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary, StringComparison.Ordinal)
            || !noLoadPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary.Contains("algorithm-family decision-anchor primary fields top 3/7", StringComparison.Ordinal)
            || !noLoadPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary.Contains("GB=1", StringComparison.Ordinal)
            || !noLoadPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary.Contains("Ps=1", StringComparison.Ordinal)
            || !noLoadPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary.Contains("R=1", StringComparison.Ordinal)
            || gb is null
            || pfw is null
            || ps is null
            || r is null)
        {
            throw new InvalidOperationException($"Motor_Y algorithm-family decision-anchor primary-field focus query smoke test mismatch. summary='{noLoadPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary}'; actual=[{string.Join(" | ", focuses.Select(x => $"{x.PrimaryField}:{x.Count}:{x.WeightedCount}:{x.WeightedShare:P1}:{string.Join("/", x.AlgorithmFamilies)}"))}]");
        }
    }

    private static void ShouldExposeRequiredResultPrimaryFieldDistributionsThroughAppQueryGateway()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-30T08:20:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-RESULT-FIELD-DIST-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.NoLoad,
                    MethodCode = "NoLoad:0",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 0,
                      "DataList": [
                        { "U": 190, "I": 6.3, "P": 520, "Cos": 0.81 }
                      ],
                      "Un": 380,
                      "K1": 1,
                      "R1c": 1.25,
                      "θ1c": 25
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y required-result primary-field distribution smoke test returned null detail.");

        var noLoadPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal));
        var distributions = noLoadPlan.RequiredResultPrimaryFieldDistributions;
        var coefficient = distributions.SingleOrDefault(x => string.Equals(x.PrimaryField, "CoefficientOfPfe", StringComparison.Ordinal));
        var pfe = distributions.SingleOrDefault(x => string.Equals(x.PrimaryField, "Pfe", StringComparison.Ordinal));
        var pfw = distributions.SingleOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));
        var r0 = distributions.SingleOrDefault(x => string.Equals(x.PrimaryField, "R0", StringComparison.Ordinal));

        if (distributions.Count != 11
            || coefficient is null
            || coefficient.Count != 2
            || !coefficient.BucketKeys.SequenceEqual(new[] { "intermediate-result-fields", "result-fields" }, StringComparer.Ordinal)
            || !coefficient.DisplayNames.SequenceEqual(new[] { "中间结果锚点", "结果字段" }, StringComparer.Ordinal)
            || pfe is null
            || pfe.Count != 2
            || !pfe.BucketKeys.SequenceEqual(new[] { "intermediate-result-fields", "result-fields" }, StringComparer.Ordinal)
            || !pfe.DisplayNames.SequenceEqual(new[] { "中间结果锚点", "结果字段" }, StringComparer.Ordinal)
            || pfw is null
            || pfw.Count != 2
            || !pfw.BucketKeys.SequenceEqual(new[] { "intermediate-result-fields", "result-fields" }, StringComparer.Ordinal)
            || !pfw.DisplayNames.SequenceEqual(new[] { "中间结果锚点", "结果字段" }, StringComparer.Ordinal)
            || r0 is null
            || r0.Count != 1
            || !r0.BucketKeys.SequenceEqual(new[] { "intermediate-result-fields" }, StringComparer.Ordinal)
            || !noLoadPlan.RequiredResultPrimaryFieldSummary.Contains("CoefficientOfPfe:2:intermediate-result-fields/result-fields", StringComparison.Ordinal)
            || !noLoadPlan.RequiredResultPrimaryFieldSummary.Contains("Pfe:2:intermediate-result-fields/result-fields", StringComparison.Ordinal)
            || !noLoadPlan.RequiredResultPrimaryFieldSummary.Contains("Pfw:2:intermediate-result-fields/result-fields", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y required-result primary-field distribution query smoke test mismatch. summary='{noLoadPlan.RequiredResultPrimaryFieldSummary}'; actual=[{string.Join(" | ", distributions.Select(x => $"{x.PrimaryField}:{x.Count}:{string.Join("/", x.BucketKeys)}:{string.Join("/", x.DisplayNames)}"))}]");
        }
    }

    private static void ShouldExposeCrossPlanRequiredResultPrimaryFieldFocusesThroughAppQueryGateway()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-30T08:20:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-CROSS-RESULT-FIELD-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.NoLoad,
                    MethodCode = "NoLoad:0",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 0,
                      "DataList": [
                        { "U": 190, "I": 6.3, "P": 520, "Cos": 0.81 }
                      ],
                      "Un": 380,
                      "K1": 1,
                      "R1c": 1.25,
                      "θ1c": 25
                    }
                    """
                },
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.LoadB,
                    MethodCode = "LoadB:5",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 5,
                      "RawDataList": [
                        { "U": 380, "I1": 10, "P1t": 500, "Nt": 1450, "Tt": 12, "Frequency": 50 }
                      ],
                      "CoefficientOfPfe": 0.97,
                      "Pfw": 120,
                      "R1c": 1.25,
                      "θ1c": 25,
                      "PolePairs": 2,
                      "Pn": 5.5,
                      "Un": 380,
                      "ΔT": 75
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y cross-plan required-result primary-field focus smoke test returned null detail.");

        var noLoadPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal));
        var loadBPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal));
        var focuses = noLoadPlan.CrossPlanRequiredResultPrimaryFieldFocuses;
        var pfw = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));
        var coefficient = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "CoefficientOfPfe", StringComparison.Ordinal));
        var pcu2 = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "Pcu2", StringComparison.Ordinal));

        if (loadBPlan.CrossPlanRequiredResultPrimaryFieldFocuses.Count != focuses.Count
            || focuses.Count < 20
            || pfw is null
            || coefficient is null
            || pcu2 is null
            || !string.Equals(loadBPlan.CrossPlanRequiredResultPrimaryFieldSummary, noLoadPlan.CrossPlanRequiredResultPrimaryFieldSummary, StringComparison.Ordinal)
            || !noLoadPlan.CrossPlanRequiredResultPrimaryFieldSummary.Contains("cross-plan required-result primary fields top 3/", StringComparison.Ordinal)
            || !noLoadPlan.CrossPlanRequiredResultPrimaryFieldSummary.Contains("Pfe=2", StringComparison.Ordinal)
            || !noLoadPlan.CrossPlanRequiredResultPrimaryFieldSummary.Contains("A=1", StringComparison.Ordinal)
            || !noLoadPlan.CrossPlanRequiredResultPrimaryFieldSummary.Contains("B=1", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y cross-plan required-result primary-field focus query smoke test mismatch. summary='{noLoadPlan.CrossPlanRequiredResultPrimaryFieldSummary}'; actual=[{string.Join(" | ", focuses.Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:{x.WeightedCount}:{x.WeightedShare:P1}:{string.Join("/", x.CanonicalCodes)}:{string.Join("/", x.SuggestedNextStepPriorities)}:{string.Join("/", x.SuggestedNextStepFocuses)}"))}]");
        }
    }

    private static void ShouldExposeAlgorithmFamilyRequiredResultPrimaryFieldFocusesThroughAppQueryGateway()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-30T08:20:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-ALGO-FAMILY-RESULT-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.NoLoad,
                    MethodCode = "NoLoad:0",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 0,
                      "DataList": [
                        { "U": 190, "I": 6.3, "P": 520, "Cos": 0.81 }
                      ],
                      "Un": 380,
                      "K1": 1,
                      "R1c": 1.25,
                      "θ1c": 25
                    }
                    """
                },
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.LoadB,
                    MethodCode = "LoadB:5",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 5,
                      "RawDataList": [
                        { "U": 380, "I1": 10, "P1t": 500, "Nt": 1450, "Tt": 12, "Frequency": 50 }
                      ],
                      "CoefficientOfPfe": 0.97,
                      "Pfw": 120,
                      "R1c": 1.25,
                      "θ1c": 25,
                      "PolePairs": 2,
                      "Pn": 5.5,
                      "Un": 380,
                      "ΔT": 75
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y algorithm-family required-result primary-field focus smoke test returned null detail.");

        var noLoadPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal));
        var loadBPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal));
        var focuses = noLoadPlan.AlgorithmFamilyRequiredResultPrimaryFieldFocuses;
        var pfw = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));
        var coefficient = focuses.SingleOrDefault(x => string.Equals(x.PrimaryField, "CoefficientOfPfe", StringComparison.Ordinal));

        if (focuses.Count < 20
            || loadBPlan.AlgorithmFamilyRequiredResultPrimaryFieldFocuses.Count != focuses.Count
            || !string.Equals(loadBPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary, noLoadPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary, StringComparison.Ordinal)
            || !noLoadPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary.Contains("algorithm-family required-result primary fields top 3/29", StringComparison.Ordinal)
            || !noLoadPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary.Contains("A=1", StringComparison.Ordinal)
            || !noLoadPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary.Contains("B=1", StringComparison.Ordinal)
            || !noLoadPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary.Contains("Nst=1", StringComparison.Ordinal)
            || !noLoadPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary.Contains("dominant-family=LoadB", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Motor_Y algorithm-family required-result primary-field focus query smoke test mismatch. summary='{noLoadPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary}'; actual=[{string.Join(" | ", focuses.Take(6).Select(x => $"{x.PrimaryField}:{x.Count}:{x.WeightedCount}:{x.WeightedShare:P1}:{string.Join("/", x.AlgorithmFamilies)}"))}]");
        }
    }

    private static void ShouldExposeVariantKindDecisionAnchorPrimaryFieldFocusesThroughAppQueryGateway()
    {
        var detail = CreateMotorYMethodAdaptationPlanDetail();

        var plans = detail.MotorYMethodAdaptationPlans.ToDictionary(x => x.CanonicalCode, StringComparer.Ordinal);
        var noLoadPlan = plans[MotorYTestMethodCodes.NoLoad];
        var loadBPlan = plans[MotorYTestMethodCodes.LoadB];

        var focuses = noLoadPlan.VariantKindDecisionAnchorPrimaryFieldFocuses;
        var baselineFocus = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));
        var deliveryFocus = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "GB", StringComparison.Ordinal));

        if (focuses.Count != 7
            || baselineFocus is null
            || deliveryFocus is null
            || loadBPlan.VariantKindDecisionAnchorPrimaryFieldFocuses.Count != focuses.Count
            || !string.Equals(noLoadPlan.VariantKindDecisionAnchorPrimaryFieldSummary, loadBPlan.VariantKindDecisionAnchorPrimaryFieldSummary, StringComparison.Ordinal)
            || !noLoadPlan.VariantKindDecisionAnchorPrimaryFieldSummary.Contains("variant-kind decision-anchor primary fields top 3/7", StringComparison.Ordinal)
            || !noLoadPlan.VariantKindDecisionAnchorPrimaryFieldSummary.Contains("CoefficientOfPfe=1", StringComparison.Ordinal)
            || !noLoadPlan.VariantKindDecisionAnchorPrimaryFieldSummary.Contains("GB=1", StringComparison.Ordinal)
            || !noLoadPlan.VariantKindDecisionAnchorPrimaryFieldSummary.Contains("Pfw=1", StringComparison.Ordinal)
            || !noLoadPlan.VariantKindDecisionAnchorPrimaryFieldSummary.Contains("dominant-variant=baseline", StringComparison.Ordinal)
            || !baselineFocus.VariantKinds.SequenceEqual(new[] { MotorYLegacyVariantKinds.Baseline }, StringComparer.Ordinal)
            || !deliveryFocus.VariantKinds.SequenceEqual(new[] { MotorYLegacyVariantKinds.Baseline }, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"TestRecordQueryGatewayAdapter variant-kind decision-anchor focus smoke test failed. summary='{noLoadPlan.VariantKindDecisionAnchorPrimaryFieldSummary}', actual=[{string.Join(" | ", focuses.Select(x => $"{x.PrimaryField}:{string.Join("/", x.VariantKinds)}:{x.WeightedCount}:{x.WeightedShare:P1}:{string.Join("/", x.CanonicalCodes)}:{string.Join("/", x.AnchorKeys)}:{x.Summary}"))}]");
        }
    }

    private static void ShouldExposeVariantKindRequiredResultPrimaryFieldFocusesThroughAppQueryGateway()
    {
        var detail = CreateMotorYMethodAdaptationPlanDetail();

        var plans = detail.MotorYMethodAdaptationPlans.ToDictionary(x => x.CanonicalCode, StringComparer.Ordinal);
        var noLoadPlan = plans[MotorYTestMethodCodes.NoLoad];
        var loadBPlan = plans[MotorYTestMethodCodes.LoadB];

        var focuses = noLoadPlan.VariantKindRequiredResultPrimaryFieldFocuses;
        var baselineFocus = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "CoefficientOfPfe", StringComparison.Ordinal));
        var pfwFocus = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));

        if (baselineFocus is null
            || pfwFocus is null
            || !baselineFocus.VariantKinds.SequenceEqual(new[] { MotorYLegacyVariantKinds.Baseline }, StringComparer.Ordinal)
            || !pfwFocus.VariantKinds.SequenceEqual(new[] { MotorYLegacyVariantKinds.Baseline }, StringComparer.Ordinal))
        {
            throw new InvalidOperationException($"TestRecordQueryGatewayAdapter variant-kind required-result focus smoke test failed. summary='{noLoadPlan.VariantKindRequiredResultPrimaryFieldSummary}', actual=[{string.Join(" | ", focuses.Select(x => $"{x.PrimaryField}:{string.Join("/", x.VariantKinds)}:{x.WeightedCount}:{x.WeightedShare:P1}:{string.Join("/", x.CanonicalCodes)}:{x.Summary}"))}]");
        }
    }

    private static void ShouldExposeHeatRunAndLoadADecisionAnchorSuggestionsThroughAppQueryGateway()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-29T11:00:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-HEATRUN-LOADA-DECISION-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.HeatRun,
                    MethodCode = "Thermal:3",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 3,
                      "Data1List": [
                        { "Time": 0, "P1": 100, "θ1": 80, "θb": 30 }
                      ],
                      "Data2List": [
                        { "Time": 30, "R": 1.2 }
                      ]
                    }
                    """
                },
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.LoadA,
                    MethodCode = "LoadA:4",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 4,
                      "RawDataList": [
                        { "U": 380, "I1": 10, "P1t": 500, "Nt": 1450, "Tt": 12, "Frequency": 50, "θ1t": 85 }
                      ]
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y HeatRun/LoadA decision-anchor smoke test returned null detail.");

        var heatRunPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.HeatRun, StringComparison.Ordinal));
        if (!heatRunPlan.SuggestedDecisionAnchorNextSteps.Any(step => step.Contains("firstSecondsInterval", StringComparison.Ordinal) && step.Contains("Pn", StringComparison.Ordinal))
            || !heatRunPlan.SuggestedDecisionAnchorNextSteps.Any(step => step.Contains("HotStateType", StringComparison.Ordinal))
            || !heatRunPlan.SuggestedDecisionAnchorNextSteps.Any(step => step.Contains("GB 温升分支", StringComparison.Ordinal) && step.Contains("GB", StringComparison.Ordinal) && step.Contains("Rn", StringComparison.Ordinal) && step.Contains("θs", StringComparison.Ordinal) && step.Contains("θw", StringComparison.Ordinal))
            || !heatRunPlan.LegacyDecisionAnchorNextActionSummary.Contains("need HeatRun firstSecondsInterval fields Pn", StringComparison.Ordinal)
            || !heatRunPlan.LegacyDecisionAnchorNextActionSummary.Contains("HotStateType", StringComparison.Ordinal)
            || !heatRunPlan.LegacyDecisionAnchorNextActionSummary.Contains("GB temperature branch fields GB, Rn, θs, θw", StringComparison.Ordinal)
            || !heatRunPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "first-seconds-interval", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "decision-interval", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "Pn" }, StringComparer.Ordinal))
            || !heatRunPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "hot-state-branch", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "legacy-branch", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "HotStateType" }, StringComparer.Ordinal))
            || !heatRunPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "gb-temperature-branch", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "legacy-branch", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "GB", "Rn", "θs", "θw" }, StringComparer.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y HeatRun decision-anchor query smoke test mismatch. next='{heatRunPlan.LegacyDecisionAnchorNextActionSummary}', suggested='{heatRunPlan.SuggestedDecisionAnchorNextStepSummary}'");
        }

        var loadAPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadA, StringComparison.Ordinal));
        if (!loadAPlan.SuggestedDecisionAnchorNextSteps.Any(step => step.Contains("payload 额定量结果字段", StringComparison.Ordinal) && step.Contains("Pcu1", StringComparison.Ordinal) && step.Contains("Pcu2", StringComparison.Ordinal) && step.Contains("η", StringComparison.Ordinal))
            || !loadAPlan.SuggestedDecisionAnchorNextSteps.Any(step => step.Contains("额定负载点回归结果", StringComparison.Ordinal) && step.Contains("ResultDataList", StringComparison.Ordinal))
            || !loadAPlan.SuggestedDecisionAnchorNextSteps.Any(step => step.Contains("上游空载/热试验承接字段", StringComparison.Ordinal) && step.Contains("CoefficientOfPfe", StringComparison.Ordinal) && step.Contains("Pfw", StringComparison.Ordinal) && step.Contains("θa", StringComparison.Ordinal))
            || !loadAPlan.LegacyDecisionAnchorNextActionSummary.Contains("payload rated-result fields Pcu1, Pcu2, η", StringComparison.Ordinal)
            || !loadAPlan.LegacyDecisionAnchorNextActionSummary.Contains("rated-load fit fields ResultDataList", StringComparison.Ordinal)
            || !loadAPlan.LegacyDecisionAnchorNextActionSummary.Contains("upstream fields CoefficientOfPfe, Pfw, θa", StringComparison.Ordinal)
            || !loadAPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "upstream-ready", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "upstream-carryover", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "CoefficientOfPfe", "Pfw", "θa" }, StringComparer.Ordinal))
            || !loadAPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "rated-load-fit-grid", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "fit-grid", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "ResultDataList" }, StringComparer.Ordinal))
            || !loadAPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "payload-rated-quantity-ready", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "rated-quantity", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "Pcu1", "Pcu2", "η" }, StringComparer.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y LoadA decision-anchor query smoke test mismatch. next='{loadAPlan.LegacyDecisionAnchorNextActionSummary}', suggested='{loadAPlan.SuggestedDecisionAnchorNextStepSummary}'");
        }
    }

    private static void ShouldExposeLoadBDecisionAnchorSuggestionsThroughAppQueryGateway()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-30T00:20:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-LOADB-DECISION-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.LoadB,
                    MethodCode = "LoadB:5",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 5,
                      "RawDataList": [
                        { "U": 380, "I1": 10, "P1t": 500, "Nt": 1450, "Tt": 12, "Frequency": 50, "θ1t": 85 }
                      ]
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y LoadB decision-anchor smoke test returned null detail.");

        var loadBPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal));
        if (!loadBPlan.SuggestedDecisionAnchorNextSteps.SequenceEqual(new[]
            {
                "先补B法坏点剔除后二次拟合证据：A, B, R, bad-point-refit",
                "先补B法 GB/ratios/θs 分支字段：GB, ratios, θs",
                "先补B法 Ps 非负迭代收敛字段：Ps, ResultDataList, cuC",
                "先补B法热态承接字段：θb, θw"
            }, StringComparer.Ordinal)
            || !string.Equals(loadBPlan.SuggestedDecisionAnchorNextStepSummary, "先补B法坏点剔除后二次拟合证据：A, B, R, bad-point-refit; 先补B法 GB/ratios/θs 分支字段：GB, ratios, θs; 先补B法 Ps 非负迭代收敛字段：Ps, ResultDataList, cuC; 先补B法热态承接字段：θb, θw", StringComparison.Ordinal)
            || !string.Equals(loadBPlan.LegacyDecisionAnchorNextActionSummary, "decision anchor next actions: need LoadB correlation refit fields A, B, R, bad-point-refit; need LoadB GB/ratios/θs branch fields GB, ratios, θs; need LoadB Ps iteration fields Ps, ResultDataList, cuC; need LoadB thermal carryover fields θb, θw", StringComparison.Ordinal)
            || !loadBPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "correlation-refit", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "regression-result", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepFocus, "B法坏点剔除后二次拟合证据", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "A", "B", "R", "bad-point-refit" }, StringComparer.Ordinal)
                && string.Equals(resolution.SuggestedNextStepSummary, "先补B法坏点剔除后二次拟合证据：A, B, R, bad-point-refit", StringComparison.Ordinal))
            || !loadBPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "gb-ratios-branch", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "legacy-branch", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepFocus, "B法 GB/ratios/θs 分支字段", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "GB", "ratios", "θs" }, StringComparer.Ordinal)
                && string.Equals(resolution.SuggestedNextStepSummary, "先补B法 GB/ratios/θs 分支字段：GB, ratios, θs", StringComparison.Ordinal))
            || !loadBPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "ps-iteration", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "iterative-convergence", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepFocus, "B法 Ps 非负迭代收敛字段", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "Ps", "ResultDataList", "cuC" }, StringComparer.Ordinal)
                && string.Equals(resolution.SuggestedNextStepSummary, "先补B法 Ps 非负迭代收敛字段：Ps, ResultDataList, cuC", StringComparison.Ordinal))
            || !loadBPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "thermal-carryover", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "upstream-carryover", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepFocus, "B法热态承接字段", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "θb", "θw" }, StringComparer.Ordinal)
                && string.Equals(resolution.SuggestedNextStepSummary, "先补B法热态承接字段：θb, θw", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y LoadB decision-anchor query smoke test mismatch. next='{loadBPlan.LegacyDecisionAnchorNextActionSummary}', suggested='{loadBPlan.SuggestedDecisionAnchorNextStepSummary}'");
        }
    }

    private static void ShouldExposeLockedRotorDecisionAnchorSuggestionsThroughAppQueryGateway()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-30T00:25:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-LOCKEDROTOR-DECISION-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.LockedRotor,
                    MethodCode = "LockedRotor:11",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 11,
                      "DataList": [
                        { "Uk": 120, "Ik": 18, "Pk": 1500, "Tk": 8, "Pkcu1": 400, "Pfe": 50, "ns": 1500 }
                      ]
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y LockedRotor decision-anchor smoke test returned null detail.");

        var lockedRotorPlan = detail.MotorYMethodAdaptationPlans.Single(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LockedRotor, StringComparison.Ordinal));
        if (!lockedRotorPlan.SuggestedDecisionAnchorNextSteps.SequenceEqual(new[]
            {
                "先补堵转 RCalType/R1s 电阻分支字段：R1s, RCalType",
                "先补堵转 TorqueCalType 分支字段：TorqueCalType",
                "先补堵转电压拟合分支基准：Un"
            }, StringComparer.Ordinal)
            || !string.Equals(lockedRotorPlan.SuggestedDecisionAnchorNextStepSummary, "先补堵转 RCalType/R1s 电阻分支字段：R1s, RCalType; 先补堵转 TorqueCalType 分支字段：TorqueCalType; 先补堵转电压拟合分支基准：Un", StringComparison.Ordinal)
            || !string.Equals(lockedRotorPlan.LegacyDecisionAnchorNextActionSummary, "decision anchor next actions: need LockedRotor RCalType/R1s fields R1s, RCalType; need LockedRotor TorqueCalType fields TorqueCalType; need LockedRotor voltage-fit branch fields Un", StringComparison.Ordinal)
            || !lockedRotorPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "rcal-branch", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "legacy-branch", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepFocus, "堵转 RCalType/R1s 电阻分支字段", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "R1s", "RCalType" }, StringComparer.Ordinal)
                && string.Equals(resolution.SuggestedNextStepSummary, "先补堵转 RCalType/R1s 电阻分支字段：R1s, RCalType", StringComparison.Ordinal))
            || !lockedRotorPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "torquecal-branch", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "legacy-branch", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepFocus, "堵转 TorqueCalType 分支字段", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "TorqueCalType" }, StringComparer.Ordinal)
                && string.Equals(resolution.SuggestedNextStepSummary, "先补堵转 TorqueCalType 分支字段：TorqueCalType", StringComparison.Ordinal))
            || !lockedRotorPlan.LegacyDecisionAnchorResolutions.Any(resolution => string.Equals(resolution.AnchorKey, "voltage-fit-branch", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepCategory, "fit-window", StringComparison.Ordinal)
                && string.Equals(resolution.SuggestedNextStepFocus, "堵转电压拟合分支基准", StringComparison.Ordinal)
                && resolution.SuggestedNextStepFields.SequenceEqual(new[] { "Un" }, StringComparer.Ordinal)
                && string.Equals(resolution.SuggestedNextStepSummary, "先补堵转电压拟合分支基准：Un", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y LockedRotor decision-anchor query smoke test mismatch. next='{lockedRotorPlan.LegacyDecisionAnchorNextActionSummary}', suggested='{lockedRotorPlan.SuggestedDecisionAnchorNextStepSummary}'");
        }
    }

    private static void ShouldExposeLegacyAlgorithmRoutePerMotorYItemWithoutBuildProfile()
    {
        var baseTime = DateTimeOffset.Parse("2026-03-28T12:30:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-ROUTE-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = baseTime,
            Items =
            {
                new()
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = "空载试验",
                    MethodCode = "NoLoad:59",
                    IsValid = true,
                    DataJson = $$"""
                    {
                      "SampleCount": 2,
                      "Method": 59,
                      "RecordMode": "key-point",
                      "DataList": [
                        { "SampleTime": "{{baseTime:O}}", "P0": 1.23 },
                        { "SampleTime": "{{baseTime.AddMinutes(1):O}}", "P0": 1.25 }
                      ]
                    }
                    """
                }
            }
        };

        var gateway = CreateGateway(record);
        var detail = gateway.GetDetailAsync(record.RecordCode).GetAwaiter().GetResult();
        if (detail is null)
        {
            throw new InvalidOperationException("Motor_Y legacy route item smoke test returned null detail.");
        }

        var item = detail.ItemDetails.Single();
        if (item.BuildProfile is not null)
        {
            throw new InvalidOperationException("Motor_Y legacy route item smoke test expected null build profile.");
        }

        if (item.LegacyAlgorithmRoute is null)
        {
            throw new InvalidOperationException("Motor_Y legacy route item smoke test missing legacy algorithm route.");
        }

        if (!string.Equals(item.ItemCode, "空载试验", StringComparison.Ordinal)
            || item.SampleCount != 2
            || !string.Equals(item.LegacyAlgorithmRoute.CanonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal)
            || item.LegacyAlgorithmRoute.MethodValue != 59
            || !string.Equals(item.LegacyAlgorithmRoute.ProfileKey, "delivery", StringComparison.Ordinal)
            || !string.Equals(item.LegacyAlgorithmRoute.LegacyFormName, MotorYLegacyFormNames.NoLoadDelivery, StringComparison.Ordinal)
            || item.LegacyAlgorithmRoute.IsBaselineMethod)
        {
            throw new InvalidOperationException("Motor_Y legacy route item smoke test route projection mismatch.");
        }
    }

    private static TestRecordDetailContract CreateMotorYMethodAdaptationPlanDetail()
    {
        var sampleTime = DateTimeOffset.Parse("2026-03-30T00:00:00+08:00");
        var record = new TestRecordAggregate
        {
            TestRecordId = Guid.NewGuid(),
            RecordCode = "REC-SMOKE-MOTORY-VARIANT-KIND-FOCUS-001",
            ProductKind = "Motor_Y",
            TestKindCode = "Routine",
            TestTime = sampleTime,
            Items =
            {
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.NoLoad,
                    MethodCode = "NoLoad:0",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 0,
                      "DataList": [
                        { "U": 380, "I0": 4.2, "P0": 320 }
                      ],
                      "P0": 320
                    }
                    """
                },
                new TestRecordItemAggregate
                {
                    TestRecordItemId = Guid.NewGuid(),
                    ItemCode = MotorYTestMethodCodes.LoadB,
                    MethodCode = "LoadB:5",
                    IsValid = true,
                    DataJson = """
                    {
                      "Method": 5,
                      "RawDataList": [
                        { "U": 380, "I1": 10, "P1t": 500, "Nt": 1450, "Tt": 12, "Frequency": 50 }
                      ],
                      "ResultDataList": [
                        { "P2": 4.1, "Pcu2": 220 }
                      ]
                    }
                    """
                }
            }
        };

        return CreateGateway(record).GetDetailAsync(record.RecordCode).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Motor_Y variant-kind primary-field focus smoke test returned null detail.");
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
