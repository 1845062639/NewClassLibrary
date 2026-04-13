using System.Text.Json;
using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class TestRecordLegacyPayloadReaderSmokeTests
{
    public static void Run()
    {
        ShouldParseLegacyPayloadSummaryFromJson();
        ShouldHandleLegacyPayloadEdgeCases();
        ShouldFormatLegacyPayloadSummaryWithMetricsFlags();
        ShouldFormatListSummaryWithPayloadFlags();
        ShouldNormalizeMotorYLegacyItemCodesFromRealStpAliases();
        ShouldResolveMotorYLegacyAliasesToStableDisplayMetadata();
        ShouldFallbackToSingleSampleForThinLoadAPayload();
        ShouldParseMotorYTrialPayloadShapesFromBuilder();
        ShouldSupportMotorYNoLoadR0ToTheta0Branch();
    }

    private static void ShouldParseLegacyPayloadSummaryFromJson()
    {
        var payload = new
        {
            SampleCount = 3,
            LegacySampleCount = 2,
            RecordMode = "legacy-replay",
            LegacySamples = new object[]
            {
                new
                {
                    LeaveFactoryModePowerCurveImage = "power-a.png",
                    LeaveFactoryModeTempCurveImage = "temp-a.png",
                    LeaveFactoryModeVibrationCurveImage = "vibration-a.png",
                    UabIncoming = 380.5,
                    Temp1 = 88.2
                },
                new
                {
                    TempRiseModePowerCurveImage = "power-b.png",
                    TempRiseModeTempCurveImage = "temp-b.png",
                    TempRiseModeVibrationFrequencyCurveImage = "vibration-b.png"
                }
            }
        };

        var snapshot = TestRecordItemPayloadReader.TryParse(JsonSerializer.Serialize(payload));

        if (snapshot.SampleCount != 3
            || snapshot.LegacySampleCount != 2
            || !snapshot.HasLegacyPayload
            || snapshot.LegacyPayload.PowerCurveImageCount != 2
            || snapshot.LegacyPayload.TempCurveImageCount != 2
            || snapshot.LegacyPayload.VibrationCurveImageCount != 2
            || !snapshot.LegacyPayload.HasIncomingPowerMetrics
            || !snapshot.LegacyPayload.HasWindingTemperatureMetrics)
        {
            throw new InvalidOperationException("Legacy payload reader smoke test failed.");
        }
    }

    private static void ShouldHandleLegacyPayloadEdgeCases()
    {
        var missingLegacySamplesSnapshot = TestRecordItemPayloadReader.TryParse(
            """
            {
              "SampleCount": 4,
              "LegacySampleCount": 2,
              "RecordMode": "legacy-replay"
            }
            """);

        if (missingLegacySamplesSnapshot.SampleCount != 4
            || missingLegacySamplesSnapshot.LegacySampleCount != 2
            || !missingLegacySamplesSnapshot.HasLegacyPayload
            || missingLegacySamplesSnapshot.LegacyPayload.PowerCurveImageCount != 0
            || missingLegacySamplesSnapshot.LegacyPayload.TempCurveImageCount != 0
            || missingLegacySamplesSnapshot.LegacyPayload.VibrationCurveImageCount != 0
            || missingLegacySamplesSnapshot.LegacyPayload.HasIncomingPowerMetrics
            || missingLegacySamplesSnapshot.LegacyPayload.HasWindingTemperatureMetrics)
        {
            throw new InvalidOperationException("Legacy payload missing-array edge case smoke test failed.");
        }

        var emptyLegacySamplesSnapshot = TestRecordItemPayloadReader.TryParse(
            """
            {
              "SampleCount": 1,
              "LegacySampleCount": 0,
              "LegacySamples": []
            }
            """);

        if (emptyLegacySamplesSnapshot.SampleCount != 1
            || emptyLegacySamplesSnapshot.LegacySampleCount != 0
            || emptyLegacySamplesSnapshot.HasLegacyPayload
            || emptyLegacySamplesSnapshot.LegacyPayload.PowerCurveImageCount != 0
            || emptyLegacySamplesSnapshot.LegacyPayload.TempCurveImageCount != 0
            || emptyLegacySamplesSnapshot.LegacyPayload.VibrationCurveImageCount != 0)
        {
            throw new InvalidOperationException("Legacy payload empty-array edge case smoke test failed.");
        }

        var partialCurvesSnapshot = TestRecordItemPayloadReader.TryParse(
            """
            {
              "SampleCount": 2,
              "LegacySampleCount": 1,
              "LegacySamples": [
                {
                  "TempRiseModePowerCurveImage": "power-only.png",
                  "Temp2": 77.3
                }
              ]
            }
            """);

        if (partialCurvesSnapshot.SampleCount != 2
            || partialCurvesSnapshot.LegacySampleCount != 1
            || !partialCurvesSnapshot.HasLegacyPayload
            || partialCurvesSnapshot.LegacyPayload.PowerCurveImageCount != 1
            || partialCurvesSnapshot.LegacyPayload.TempCurveImageCount != 0
            || partialCurvesSnapshot.LegacyPayload.VibrationCurveImageCount != 0
            || partialCurvesSnapshot.LegacyPayload.HasIncomingPowerMetrics
            || !partialCurvesSnapshot.LegacyPayload.HasWindingTemperatureMetrics)
        {
            throw new InvalidOperationException("Legacy payload partial-curves edge case smoke test failed.");
        }
    }

    private static void ShouldFormatLegacyPayloadSummaryWithMetricsFlags()
    {
        var summary = FormatPayloadSnapshotSummary(new[]
        {
            (
                ItemCode: "Noise",
                Payload: new TestRecordItemPayloadSnapshot
                {
                    LegacySampleCount = 2,
                    LegacyPayload = new TestRecordLegacyPayloadSummary
                    {
                        LegacySampleCount = 2,
                        PowerCurveImageCount = 1,
                        TempCurveImageCount = 1,
                        VibrationCurveImageCount = 1,
                        HasIncomingPowerMetrics = true,
                        HasWindingTemperatureMetrics = false
                    }
                })
        });

        const string expected = "Noise:legacy=2:power=1:temp=1:vibration=1:incoming=Y:winding=N";
        if (!string.Equals(summary, expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Legacy payload formatter smoke test failed. Expected '{expected}', got '{summary}'.");
        }
    }

    private static void ShouldFormatListSummaryWithPayloadFlags()
    {
        var summary = TestRecordLegacyPayloadFormatter.FormatListSummary(new[]
        {
            new TestRecordItemPartitionContract
            {
                ItemCode = "Noise",
                LegacySampleCount = 2,
                HasLegacyPayload = true
            },
            new TestRecordItemPartitionContract
            {
                ItemCode = "TempRise",
                LegacySampleCount = 0,
                HasLegacyPayload = false
            }
        });

        const string expected = "Noise:legacy=2:payload=Y, TempRise:legacy=0:payload=N";
        if (!string.Equals(summary, expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Legacy payload list formatter smoke test failed. Expected '{expected}', got '{summary}'.");
        }
    }

    private static void ShouldNormalizeMotorYLegacyItemCodesFromRealStpAliases()
    {
        var cases = new (string Alias, string Canonical, bool IsCoreTrial)[]
        {
            ("直流电阻测定", MotorYTestMethodCodes.DcResistance, true),
            ("陪试直流电阻测定", MotorYTestMethodCodes.DcResistance, true),
            ("空载特性试验", MotorYTestMethodCodes.NoLoad, true),
            ("空载试验", MotorYTestMethodCodes.NoLoad, true),
            ("空载试验（出厂）", MotorYTestMethodCodes.NoLoad, true),
            ("空载特性测量", MotorYTestMethodCodes.NoLoad, true),
            ("空载特性完全试验", MotorYTestMethodCodes.NoLoad, true),
            ("热试验", MotorYTestMethodCodes.HeatRun, true),
            ("热试验2", MotorYTestMethodCodes.HeatRun, true),
            ("温度计法热试验", MotorYTestMethodCodes.HeatRun, true),
            ("陪试热试验", MotorYTestMethodCodes.HeatRun, true),
            ("A法负载试验", MotorYTestMethodCodes.LoadA, true),
            ("B法负载试验", MotorYTestMethodCodes.LoadB, true),
            ("堵转特性试验", MotorYTestMethodCodes.LockedRotor, true),
            ("堵转试验", MotorYTestMethodCodes.LockedRotor, true),
            ("堵转试验（出厂）", MotorYTestMethodCodes.LockedRotor, true),
            ("C法负载试验", "C法负载试验", false)
        };

        foreach (var (alias, canonical, isCoreTrial) in cases)
        {
            var normalized = MotorYLegacyItemCodeNormalizer.Normalize(alias);
            if (!string.Equals(normalized, canonical, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y alias normalize smoke test failed for '{alias}'. Expected '{canonical}', got '{normalized}'.");
            }

            if (MotorYLegacyItemCodeNormalizer.IsMotorYCoreTrial(alias) != isCoreTrial)
            {
                throw new InvalidOperationException($"Motor_Y core-trial detect smoke test failed for '{alias}'.");
            }
        }
    }

    private static void ShouldResolveMotorYLegacyAliasesToStableDisplayMetadata()
    {
        var cases = new (string Alias, string DisplayName, int SortOrder)[]
        {
            ("直流电阻测定", "Motor_Y DC Resistance", 10),
            ("空载特性试验", "Motor_Y No-Load Test", 20),
            ("热试验2", "Motor_Y Heat Run Test", 30),
            ("A法负载试验", "Motor_Y Load Test A", 40),
            ("B法负载试验", "Motor_Y Load Test B", 50),
            ("堵转试验（出厂）", "Motor_Y Locked-Rotor Test", 60)
        };

        foreach (var (alias, displayName, sortOrder) in cases)
        {
            var resolvedName = TestRecordItemDescriptorResolver.ResolveDisplayName(alias, recordMode: null);
            var resolvedSortOrder = TestRecordItemDescriptorResolver.ResolveSortOrder(alias, recordMode: null);
            if (!string.Equals(resolvedName, displayName, StringComparison.Ordinal) || resolvedSortOrder != sortOrder)
            {
                throw new InvalidOperationException(
                    $"Motor_Y descriptor resolver smoke test failed for '{alias}'. Expected name='{displayName}', sort={sortOrder}; got name='{resolvedName}', sort={resolvedSortOrder}.");
            }
        }
    }

    private static void ShouldFallbackToSingleSampleForThinLoadAPayload()
    {
        const string thinLoadAPayload =
            """
            {
              "Un": 380,
              "Pn": 132,
              "K1": 235,
              "R1c": 0.1583,
              "θ1c": 26.5,
              "θa": 26.5,
              "ΔT": 0.35,
              "Order": 3,
              "DecimalPlaces": 2,
              "RawDataList": [],
              "ResultDataList": []
            }
            """;

        var snapshot = TestRecordItemPayloadReader.TryParse(thinLoadAPayload);
        if (snapshot.SampleCount != 1 || snapshot.RecordMode != TestRecordSampleModes.Continuous)
        {
            throw new InvalidOperationException(
                $"Thin LoadA payload fallback smoke test failed. Expected sampleCount=1, recordMode={TestRecordSampleModes.Continuous}; got sampleCount={snapshot.SampleCount}, recordMode={snapshot.RecordMode ?? "<null>"}.");
        }
    }

    private static void ShouldParseMotorYTrialPayloadShapesFromBuilder()
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

        var baseTime = DateTimeOffset.Parse("2026-03-28T10:00:00+08:00");
        var samples = new[]
        {
            new MotorRealtimeSampleContract
            {
                SampleTime = baseTime,
                ProductKind = "Motor_Y",
                VoltageAverage = 120.0,
                CurrentAverage = 40.0,
                Power = 12.0,
                Frequency = 50,
                Speed = 1492,
                Torque = 80.0,
                IsRecordPoint = true
            },
            new MotorRealtimeSampleContract
            {
                SampleTime = baseTime.AddSeconds(10),
                ProductKind = "Motor_Y",
                VoltageAverage = 380.0,
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
                VoltageAverage = 420.0,
                CurrentAverage = 74.1,
                Power = 36.7,
                Frequency = 50,
                Speed = 1468,
                Torque = 149.3,
                IsRecordPoint = true
            }
        };

        var items = builder.BuildTrialItems(rated, samples).ToDictionary(x => x.ItemCode, StringComparer.Ordinal);

        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.DcResistance, expectedSampleCount: 1, expectedRecordMode: null);
        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.NoLoad, expectedSampleCount: 3, expectedRecordMode: TestRecordSampleModes.KeyPointOnly);
        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.HeatRun, expectedSampleCount: 3, expectedRecordMode: TestRecordSampleModes.Continuous);
        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.LoadA, expectedSampleCount: 2, expectedRecordMode: TestRecordSampleModes.Continuous);
        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.LoadB, expectedSampleCount: 1, expectedRecordMode: TestRecordSampleModes.Continuous);
        AssertMotorYPayloadSnapshot(items, MotorYTestMethodCodes.LockedRotor, expectedSampleCount: 2, expectedRecordMode: TestRecordSampleModes.KeyPointOnly);

        AssertMotorYNoLoadBuilderAggregation(items[MotorYTestMethodCodes.NoLoad]);
        AssertMotorYNoLoadExecutionEntry(items[MotorYTestMethodCodes.NoLoad]);
    }

    private static void ShouldSupportMotorYNoLoadR0ToTheta0Branch()
    {
        const double un = 380.0;
        const double r1c = 0.18;
        const double theta1c = 25.0;
        const double k1 = 235.0;
        const double initialR0 = 0.205;
        const int decimalPlaces = 4;
        const int order = 3;

        var dataJson = JsonSerializer.Serialize(new
        {
            Un = un,
            R1c = r1c,
            θ1c = theta1c,
            K1 = k1,
            Order = order,
            DecimalPlaces = decimalPlaces,
            RConverseType = 1,
            R0 = initialR0,
            DataList = new[]
            {
                new
                {
                    U0 = 180.0,
                    U0DivideUn = 180.0 / un,
                    U0DivideUnSquare = Math.Pow(180.0 / un, 2),
                    I0 = 41.2,
                    I01 = 40.8,
                    I02 = 41.3,
                    I03 = 41.5,
                    P0 = 16.8,
                    Cosφ = 0.73,
                    Frequency = 50.0,
                    θ0 = 26.5,
                    R0 = initialR0,
                    ΔI0 = 0.85,
                    P0cu1 = 0.0,
                    Pcon = 0.0,
                    Pfe = 0.0,
                    n0 = 1496.0,
                    T0 = 18.2
                },
                new
                {
                    U0 = 380.0,
                    U0DivideUn = 1.0,
                    U0DivideUnSquare = 1.0,
                    I0 = 66.4,
                    I01 = 65.9,
                    I02 = 66.5,
                    I03 = 66.8,
                    P0 = 31.2,
                    Cosφ = 0.76,
                    Frequency = 50.0,
                    θ0 = 27.7,
                    R0 = initialR0,
                    ΔI0 = 0.92,
                    P0cu1 = 0.0,
                    Pcon = 0.0,
                    Pfe = 0.0,
                    n0 = 1490.0,
                    T0 = 24.6
                },
                new
                {
                    U0 = 420.0,
                    U0DivideUn = 420.0 / un,
                    U0DivideUnSquare = Math.Pow(420.0 / un, 2),
                    I0 = 74.1,
                    I01 = 73.7,
                    I02 = 74.0,
                    I03 = 74.6,
                    P0 = 36.7,
                    Cosφ = 0.78,
                    Frequency = 50.0,
                    θ0 = 28.4,
                    R0 = initialR0,
                    ΔI0 = 0.97,
                    P0cu1 = 0.0,
                    Pcon = 0.0,
                    Pfe = 0.0,
                    n0 = 1486.0,
                    T0 = 27.1
                }
            }
        });

        var shape = MotorYNoLoadLegacyShape.FromJson(dataJson)
            ?? throw new InvalidOperationException("Motor_Y NoLoad R0->θ0 smoke test failed: shape parse returned null.");

        var computation = MotorYNoLoadComputation.Compute(
            shape.DataList.Select(x => new MotorYNoLoadComputedPoint
            {
                U0 = x.U0,
                U0DivideUn = x.U0DivideUn,
                U0DivideUnSquare = x.U0DivideUnSquare,
                I0 = x.I0,
                P0 = x.P0,
                Theta0 = x.θ0,
                R0 = x.R0,
                DeltaI0 = x.ΔI0,
                P0cu1 = x.P0cu1,
                Pcon = x.Pcon
            }).ToArray(),
            shape.Un,
            shape.Order,
            shape.DecimalPlaces,
            shape.R1c,
            shape.θ1c,
            shape.K1,
            shape.RConverseType,
            shape.R0);

        var expectedTheta0 = Math.Round(initialR0 / r1c * (k1 + theta1c) - k1, decimalPlaces);
        var expectedR0 = Math.Round(initialR0, decimalPlaces);
        var execution = MotorYNoLoadExecutionAdapter.Build(dataJson);

        if (!execution.IsExecutable
            || !string.Equals(execution.RConverseBranch, "R0->θ0", StringComparison.Ordinal)
            || Math.Abs(execution.ComputedTheta0 - expectedTheta0) > 0.0001
            || Math.Abs(execution.ComputedR0 - expectedR0) > 0.0001
            || Math.Abs(execution.ComputedTheta0 - computation.ComputedTheta0) > 0.0001
            || Math.Abs(execution.ComputedR0 - computation.ComputedR0) > 0.0001
            || Math.Abs(execution.RatedVoltagePointVoltage - 380.0) > 0.0001
            || execution.PfwFitSampleCount != 1
            || execution.PfwFitWindowReady)
        {
            throw new InvalidOperationException(
                $"Motor_Y NoLoad R0->θ0 smoke test failed. branch={execution.RConverseBranch}, executable={execution.IsExecutable}, θ0={execution.ComputedTheta0}, R0={execution.ComputedR0}, expectedθ0={expectedTheta0}, expectedR0={expectedR0}, ratedU0={execution.RatedVoltagePointVoltage}, pfw-fit={execution.PfwFitSampleCount}/{execution.PfwFitWindowReady}.");
        }
    }

    private static void AssertMotorYNoLoadExecutionEntry(StandardTestNext.Test.Domain.Records.TestRecordItemAggregate item)
    {
        var execution = MotorYNoLoadExecutionAdapter.Build(item.DataJson);
        if (!execution.IsExecutable
            || !string.Equals(execution.ExecutionStage, "rconverse+rated-point-only", StringComparison.Ordinal)
            || !string.Equals(execution.RConverseBranch, "θ0->R0", StringComparison.Ordinal)
            || Math.Abs(execution.ComputedTheta0 - 27.7) > 0.0001
            || Math.Abs(execution.ComputedR0 - 0.1908) > 0.0001
            || Math.Abs(execution.RatedVoltagePointVoltage - 380.0) > 0.0001
            || Math.Abs(execution.RatedVoltagePointCurrent - 66.4) > 0.0001
            || Math.Abs(execution.RatedVoltagePointPower - 31.2) > 0.0001
            || execution.PfwFitWindowReady
            || execution.PfwFitSampleCount != 1)
        {
            throw new InvalidOperationException(
                $"Motor_Y NoLoad execution entry smoke test failed. stage={execution.ExecutionStage}, executable={execution.IsExecutable}, branch={execution.RConverseBranch}, θ0={execution.ComputedTheta0}, R0={execution.ComputedR0}, rated-point={execution.RatedVoltagePointVoltage}/{execution.RatedVoltagePointCurrent}/{execution.RatedVoltagePointPower}, pfw={execution.PfwEstimate}, pfe={execution.PfeEstimate}, pfw-fit={execution.PfwFitSampleCount}/{execution.PfwFitWindowReady}, missing=[{string.Join(", ", execution.MissingInputs)}].");
        }
    }

    private static void AssertMotorYNoLoadBuilderAggregation(StandardTestNext.Test.Domain.Records.TestRecordItemAggregate item)
    {
        var shape = MotorYNoLoadLegacyShape.FromJson(item.DataJson);
        if (shape is null)
        {
            throw new InvalidOperationException("Motor_Y NoLoad builder aggregation smoke test failed: legacy shape parse returned null.");
        }

        if (shape.DataList.Count != 3)
        {
            throw new InvalidOperationException($"Motor_Y NoLoad builder aggregation smoke test failed: expected 3 data points, got {shape.DataList.Count}.");
        }

        var ratedPoint = shape.DataList.OrderBy(x => Math.Abs(x.U0DivideUn - 1d)).First();
        var computation = MotorYNoLoadComputation.Compute(
            shape.DataList.Select(x => new MotorYNoLoadComputedPoint
            {
                U0 = x.U0,
                U0DivideUn = x.U0DivideUn,
                U0DivideUnSquare = x.U0DivideUnSquare,
                I0 = x.I0,
                P0 = x.P0,
                Theta0 = x.θ0,
                R0 = x.R0,
                DeltaI0 = x.ΔI0,
                P0cu1 = x.P0cu1,
                Pcon = x.Pcon
            }).ToArray(),
            shape.Un,
            shape.Order,
            shape.DecimalPlaces,
            shape.R1c,
            shape.θ1c,
            shape.K1,
            shape.RConverseType,
            shape.R0);
        var expectedPfw = computation.Pfw;
        var expectedPfe = computation.Pfe;
        var expectedTheta0Values = shape.DataList
            .Select((x, index) => Math.Round(26.5 + index * 0.6, shape.DecimalPlaces))
            .ToArray();
        var expectedR0Values = expectedTheta0Values
            .Select(theta0 => Math.Round(shape.R1c * (shape.K1 + theta0) / (shape.K1 + shape.θ1c), shape.DecimalPlaces))
            .ToArray();
        var expectedP0cu1Values = shape.DataList
            .Select((x, index) => Math.Round(1.5 * expectedR0Values[index] * x.I0 * x.I0, shape.DecimalPlaces))
            .ToArray();
        var expectedPconValues = shape.DataList
            .Select((x, index) => Math.Round(x.P0 - expectedP0cu1Values[index], shape.DecimalPlaces))
            .ToArray();
        var expectedDeltaI0Values = shape.DataList
            .Select(x =>
            {
                var average = (x.I01 + x.I02 + x.I03) / 3d;
                return average <= 0d
                    ? 0d
                    : Math.Round(new[]
                    {
                        Math.Abs(x.I01 - average),
                        Math.Abs(x.I02 - average),
                        Math.Abs(x.I03 - average)
                    }.Max() / average * 100d, shape.DecimalPlaces);
            })
            .ToArray();

        if (shape.DataList.Select(x => x.θ0).Zip(expectedTheta0Values, (actual, expected) => Math.Abs(actual - expected) <= 0.0001).Any(match => !match))
        {
            throw new InvalidOperationException(
                $"Motor_Y NoLoad builder aggregation smoke test failed: θ0 per-point values are not using the staged no-load branch sequence. actual=[{string.Join(", ", shape.DataList.Select(x => x.θ0))}], expected=[{string.Join(", ", expectedTheta0Values)}].");
        }

        if (shape.DataList.Select(x => x.R0).Zip(expectedR0Values, (actual, expected) => Math.Abs(actual - expected) <= 0.0001).Any(match => !match))
        {
            throw new InvalidOperationException(
                $"Motor_Y NoLoad builder aggregation smoke test failed: R0 per-point values are not recomputed from θ0 branch semantics. actual=[{string.Join(", ", shape.DataList.Select(x => x.R0))}], expected=[{string.Join(", ", expectedR0Values)}].");
        }

        if (shape.DataList.Select(x => x.P0cu1).Zip(expectedP0cu1Values, (actual, expected) => Math.Abs(actual - expected) <= 0.0001).Any(match => !match))
        {
            throw new InvalidOperationException(
                $"Motor_Y NoLoad builder aggregation smoke test failed: P0cu1 per-point values are not recomputed from R0 and I0. actual=[{string.Join(", ", shape.DataList.Select(x => x.P0cu1))}], expected=[{string.Join(", ", expectedP0cu1Values)}].");
        }

        if (shape.DataList.Select(x => x.Pcon).Zip(expectedPconValues, (actual, expected) => Math.Abs(actual - expected) <= 0.0001).Any(match => !match))
        {
            throw new InvalidOperationException(
                $"Motor_Y NoLoad builder aggregation smoke test failed: Pcon per-point values are not recomputed from P0-P0cu1. actual=[{string.Join(", ", shape.DataList.Select(x => x.Pcon))}], expected=[{string.Join(", ", expectedPconValues)}].");
        }

        if (shape.DataList.Select(x => x.ΔI0).Zip(expectedDeltaI0Values, (actual, expected) => Math.Abs(actual - expected) <= 0.0001).Any(match => !match))
        {
            throw new InvalidOperationException(
                $"Motor_Y NoLoad builder aggregation smoke test failed: ΔI0 per-point values are not derived from I01/I02/I03. actual=[{string.Join(", ", shape.DataList.Select(x => x.ΔI0))}], expected=[{string.Join(", ", expectedDeltaI0Values)}].");
        }

        if (Math.Abs(shape.Pfw - expectedPfw) > 0.0001
            || Math.Abs(shape.Pfe - expectedPfe) > 0.0001
            || Math.Abs(shape.I0 - computation.FittedI0AtRated) > 0.0001
            || Math.Abs(shape.P0 - computation.FittedP0AtRated) > 0.0001
            || Math.Abs(shape.ΔI0 - computation.FittedDeltaI0AtRated) > 0.0001
            || Math.Abs(shape.Pcu - computation.FittedPcuAtRated) > 0.0001
            || shape.CoefficientOfPfe.Length != computation.CoefficientOfPfe.Length
            || shape.CoefficientOfPfe.Zip(computation.CoefficientOfPfe, (actual, expected) => Math.Abs(actual - expected) <= 0.0001).Any(match => !match)
            || Math.Abs(shape.U0DivideUnIsEquesToOne_I0 - computation.FittedI0AtRated) > 0.0001
            || Math.Abs(shape.U0DivideUnIsEquesToOne_P0 - computation.FittedP0AtRated) > 0.0001
            || Math.Abs(shape.U0DivideUnIsEquesToOne_Pcu - computation.FittedPcuAtRated) > 0.0001
            || Math.Abs(shape.U0DivideUnIsEquesToOne_Pfe - expectedPfe) > 0.0001
            || shape.PfwFitSampleCount != 1
            || shape.PfwFitWindowReady)
        {
            throw new InvalidOperationException(
                $"Motor_Y NoLoad builder aggregation smoke test failed. Expected Pfw={expectedPfw}, Pfe={expectedPfe}, fitted(I0={computation.FittedI0AtRated}, P0={computation.FittedP0AtRated}, ΔI0={computation.FittedDeltaI0AtRated}, Pcu={computation.FittedPcuAtRated}), ratedPoint(U0={ratedPoint.U0}, I0={ratedPoint.I0}, P0={ratedPoint.P0}); got Pfw={shape.Pfw}, Pfe={shape.Pfe}, top(I0={shape.I0}, P0={shape.P0}, ΔI0={shape.ΔI0}, Pcu={shape.Pcu}), coeff=[{string.Join(", ", shape.CoefficientOfPfe)}], pu1(I0={shape.U0DivideUnIsEquesToOne_I0}, P0={shape.U0DivideUnIsEquesToOne_P0}, Pcu={shape.U0DivideUnIsEquesToOne_Pcu}, Pfe={shape.U0DivideUnIsEquesToOne_Pfe}), pfw-fit(samples={shape.PfwFitSampleCount}, ready={shape.PfwFitWindowReady}).");
        }
    }

    private static void AssertMotorYPayloadSnapshot(
        IReadOnlyDictionary<string, StandardTestNext.Test.Domain.Records.TestRecordItemAggregate> items,
        string itemCode,
        int expectedSampleCount,
        string? expectedRecordMode)
    {
        if (!items.TryGetValue(itemCode, out var item))
        {
            throw new InvalidOperationException($"Motor_Y builder smoke test missing item '{itemCode}'.");
        }

        var snapshot = TestRecordItemPayloadReader.TryParse(item.DataJson);
        if (snapshot.SampleCount != expectedSampleCount || !string.Equals(snapshot.RecordMode, expectedRecordMode, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Motor_Y payload reader smoke test failed for '{itemCode}'. Expected sampleCount={expectedSampleCount}, recordMode={expectedRecordMode ?? "<null>"}; got sampleCount={snapshot.SampleCount}, recordMode={snapshot.RecordMode ?? "<null>"}.");
        }
    }

    private static string FormatPayloadSnapshotSummary(IEnumerable<(string ItemCode, TestRecordItemPayloadSnapshot Payload)> snapshots)
    {
        return string.Join(", ",
            snapshots.Select(x => $"{x.ItemCode}:legacy={x.Payload.LegacySampleCount}:power={x.Payload.LegacyPayload.PowerCurveImageCount}:temp={x.Payload.LegacyPayload.TempCurveImageCount}:vibration={x.Payload.LegacyPayload.VibrationCurveImageCount}:incoming={(x.Payload.LegacyPayload.HasIncomingPowerMetrics ? "Y" : "N")}:winding={(x.Payload.LegacyPayload.HasWindingTemperatureMetrics ? "Y" : "N")}"));
    }
}
