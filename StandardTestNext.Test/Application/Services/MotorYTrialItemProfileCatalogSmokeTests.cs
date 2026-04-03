using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public static class MotorYTrialItemProfileCatalogSmokeTests
{
    public static void Run()
    {
        ShouldResolveBaselineProfilesForAllCoreTrials();
        ShouldStampBuilderItemsWithBaselineRouteMetadata();
    }

    private static void ShouldResolveBaselineProfilesForAllCoreTrials()
    {
        var expected = new[]
        {
            (MotorYTestMethodCodes.DcResistance, 1, MotorYLegacyVariantKinds.Baseline, MotorYLegacyAlgorithmFamilies.DirectCurrentResistance),
            (MotorYTestMethodCodes.NoLoad, 0, MotorYLegacyVariantKinds.Baseline, MotorYLegacyAlgorithmFamilies.NoLoad),
            (MotorYTestMethodCodes.HeatRun, 3, MotorYLegacyVariantKinds.Baseline, MotorYLegacyAlgorithmFamilies.Thermal),
            (MotorYTestMethodCodes.LoadA, 4, MotorYLegacyVariantKinds.Baseline, MotorYLegacyAlgorithmFamilies.LoadA),
            (MotorYTestMethodCodes.LoadB, 5, MotorYLegacyVariantKinds.Baseline, MotorYLegacyAlgorithmFamilies.LoadB),
            (MotorYTestMethodCodes.LockedRotor, 11, MotorYLegacyVariantKinds.Baseline, MotorYLegacyAlgorithmFamilies.LockedRotor)
        };

        foreach (var row in expected)
        {
            var profile = MotorYTrialItemProfileCatalog.GetRequiredBaseline(row.Item1);
            if (profile.MethodValue != row.Item2
                || !profile.IsBaselineMethod
                || !string.Equals(profile.VariantKind, row.Item3, StringComparison.Ordinal)
                || !string.Equals(profile.AlgorithmFamily, row.Item4, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(profile.LegacyAlgorithmEntry)
                || string.IsNullOrWhiteSpace(profile.LegacyMethodName)
                || string.IsNullOrWhiteSpace(profile.LegacySettingsMethodName))
            {
                throw new InvalidOperationException($"Motor_Y baseline build profile mismatch for {row.Item1}.");
            }
        }
    }

    private static void ShouldStampBuilderItemsWithBaselineRouteMetadata()
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

        var baseTime = DateTimeOffset.Parse("2026-03-28T11:00:00+08:00");
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

        var items = builder.BuildTrialItems(rated, samples);
        foreach (var item in items)
        {
            if (!MotorYTrialItemProfileCatalog.BaselineProfiles.TryGetValue(item.MethodCode, out var expectedProfile))
            {
                throw new InvalidOperationException($"Motor_Y builder item resolved unknown method code {item.MethodCode}.");
            }

            if (item.MethodValue != expectedProfile.MethodValue
                || item.BuildProfile is null
                || !string.Equals(item.BuildProfile.MethodKey, expectedProfile.MethodKey, StringComparison.Ordinal)
                || !string.Equals(item.BuildProfile.ProfileKey, expectedProfile.ProfileKey, StringComparison.Ordinal)
                || !string.Equals(item.BuildProfile.LegacyEnumName, expectedProfile.LegacyEnumName, StringComparison.Ordinal)
                || !string.Equals(item.BuildProfile.LegacyFormName, expectedProfile.LegacyFormName, StringComparison.Ordinal)
                || !string.Equals(item.BuildProfile.LegacyAlgorithmEntry, expectedProfile.LegacyAlgorithmEntry, StringComparison.Ordinal)
                || !string.Equals(item.BuildProfile.VariantKind, MotorYLegacyVariantKinds.Baseline, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y builder item baseline route metadata mismatch for {item.ItemCode}.");
            }
        }
    }
}
