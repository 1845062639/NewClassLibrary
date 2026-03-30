using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace StandardTestNext.Test.Application.Services;

public static class StpDbMotorYMethodAdaptationPlanSmokeTests
{
    private const double DominantOverrideThreshold = 0.7d;
    private static readonly string DbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "stp.db"));

    public static void Run()
    {
        if (!File.Exists(DbPath))
        {
            throw new InvalidOperationException($"stp.db not found for method adaptation plan smoke test: {DbPath}");
        }

        var service = new StpDbSnapshotQueryService(DbPath);
        var actual = service.ListMotorYMethodAdaptationPlans();
        if (actual.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: no rows returned.");
        }

        var legacyCodeDistributionRows = service.ListMotorYLegacyCodeDistribution();
        if (legacyCodeDistributionRows.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: no legacy-code distribution rows returned.");
        }

        var crossPlanPrimaryFieldFocuses = service.ListMotorYDecisionAnchorPrimaryFieldFocuses();
        if (crossPlanPrimaryFieldFocuses.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: no cross-plan decision-anchor primary-field focuses returned.");
        }

        var crossPlanRequiredResultPrimaryFieldFocuses = service.ListMotorYRequiredResultPrimaryFieldFocuses();
        if (crossPlanRequiredResultPrimaryFieldFocuses.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: no cross-plan required-result primary-field focuses returned.");
        }

        var algorithmFamilyDecisionAnchorPrimaryFieldFocuses = service.ListMotorYAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses();
        if (algorithmFamilyDecisionAnchorPrimaryFieldFocuses.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: no algorithm-family decision-anchor primary-field focuses returned.");
        }

        var algorithmFamilyRequiredResultPrimaryFieldFocuses = service.ListMotorYAlgorithmFamilyRequiredResultPrimaryFieldFocuses();
        if (algorithmFamilyRequiredResultPrimaryFieldFocuses.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: no algorithm-family required-result primary-field focuses returned.");
        }

        var variantKindDecisionAnchorPrimaryFieldFocuses = service.ListMotorYVariantKindDecisionAnchorPrimaryFieldFocuses();
        if (variantKindDecisionAnchorPrimaryFieldFocuses.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: no variant-kind decision-anchor primary-field focuses returned.");
        }

        var variantKindRequiredResultPrimaryFieldFocuses = service.ListMotorYVariantKindRequiredResultPrimaryFieldFocuses();
        if (variantKindRequiredResultPrimaryFieldFocuses.Count == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: no variant-kind required-result primary-field focuses returned.");
        }

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        var expected = new[]
        {
            BuildExpected(connection, MotorYTestMethodCodes.DcResistance, 1),
            BuildExpected(connection, MotorYTestMethodCodes.NoLoad, 0),
            BuildExpected(connection, MotorYTestMethodCodes.HeatRun, 3),
            BuildExpected(connection, MotorYTestMethodCodes.LoadA, 4),
            BuildExpected(connection, MotorYTestMethodCodes.LoadB, 5),
            BuildExpected(connection, MotorYTestMethodCodes.LockedRotor, 11)
        };

        AssertCrossPlanDecisionAnchorPrimaryFieldFocuses(actual, crossPlanPrimaryFieldFocuses);
        AssertCrossPlanDecisionAnchorPrimaryFieldFocusSummaries(actual, crossPlanPrimaryFieldFocuses);
        AssertCrossPlanRequiredResultPrimaryFieldFocuses(actual, crossPlanRequiredResultPrimaryFieldFocuses);
        AssertCrossPlanRequiredResultPrimaryFieldFocusSummaries(actual, crossPlanRequiredResultPrimaryFieldFocuses);
        AssertAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses(actual, algorithmFamilyDecisionAnchorPrimaryFieldFocuses);
        AssertAlgorithmFamilyDecisionAnchorPrimaryFieldFocusSummaries(actual, algorithmFamilyDecisionAnchorPrimaryFieldFocuses);
        AssertAlgorithmFamilyRequiredResultPrimaryFieldFocuses(actual, algorithmFamilyRequiredResultPrimaryFieldFocuses);
        AssertAlgorithmFamilyRequiredResultPrimaryFieldFocusSummaries(actual, algorithmFamilyRequiredResultPrimaryFieldFocuses);
        AssertVariantKindDecisionAnchorPrimaryFieldFocuses(actual, variantKindDecisionAnchorPrimaryFieldFocuses);
        AssertVariantKindDecisionAnchorPrimaryFieldFocusSummaries(actual, variantKindDecisionAnchorPrimaryFieldFocuses);
        AssertVariantKindRequiredResultPrimaryFieldFocuses(actual, variantKindRequiredResultPrimaryFieldFocuses);
        AssertVariantKindRequiredResultPrimaryFieldFocusSummaries(actual, variantKindRequiredResultPrimaryFieldFocuses);

        var familyResultCoefficient = algorithmFamilyRequiredResultPrimaryFieldFocuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "CoefficientOfPfe", StringComparison.Ordinal));
        var familyResultPfw = algorithmFamilyRequiredResultPrimaryFieldFocuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));
        var familyResultPcu2 = algorithmFamilyRequiredResultPrimaryFieldFocuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "Pcu2", StringComparison.Ordinal));
        if (familyResultCoefficient is null
            || familyResultPfw is null
            || familyResultPcu2 is null
            || familyResultCoefficient.Count != 2
            || Math.Abs(familyResultCoefficient.Share - 0.3333d) > 0.0001d
            || familyResultCoefficient.WeightedCount != 492
            || Math.Abs(familyResultCoefficient.WeightedShare - 0.2293d) > 0.0001d
            || !familyResultCoefficient.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.LoadB, MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal)
            || !familyResultCoefficient.AlgorithmFamilies.SequenceEqual(new[] { "LoadB", "NoLoad" }, StringComparer.Ordinal)
            || !string.Equals(familyResultCoefficient.Summary, "family=LoadB; cross-plan primary field CoefficientOfPfe appears in 1/1 plans (100pp), weighted 61/61 selected samples (100pp); codes=MotorY.LoadB; families=LoadB; focuses=结果字段; priorities=result-fields", StringComparison.Ordinal)
            || familyResultPfw.Count != 2
            || Math.Abs(familyResultPfw.Share - 0.3333d) > 0.0001d
            || familyResultPfw.WeightedCount != 492
            || Math.Abs(familyResultPfw.WeightedShare - 0.2293d) > 0.0001d
            || !familyResultPfw.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.LoadB, MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal)
            || !familyResultPfw.AlgorithmFamilies.SequenceEqual(new[] { "LoadB", "NoLoad" }, StringComparer.Ordinal)
            || !string.Equals(familyResultPfw.Summary, "family=LoadB; cross-plan primary field Pfw appears in 1/1 plans (100pp), weighted 61/61 selected samples (100pp); codes=MotorY.LoadB; families=LoadB; focuses=中间结果锚点, 结果字段; priorities=intermediate-result-fields, result-fields", StringComparison.Ordinal)
            || familyResultPcu2.Count != 2
            || Math.Abs(familyResultPcu2.Share - 0.3333d) > 0.0001d
            || familyResultPcu2.WeightedCount != 491
            || Math.Abs(familyResultPcu2.WeightedShare - 0.2289d) > 0.0001d
            || !familyResultPcu2.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.LoadA, MotorYTestMethodCodes.LoadB }, StringComparer.Ordinal)
            || !familyResultPcu2.AlgorithmFamilies.SequenceEqual(new[] { "LoadA", "LoadB" }, StringComparer.Ordinal)
            || !string.Equals(familyResultPcu2.Summary, "family=LoadA; cross-plan primary field Pcu2 appears in 1/1 plans (100pp), weighted 430/430 selected samples (100pp); codes=MotorY.LoadA; families=LoadA; focuses=结果字段; priorities=result-fields", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: algorithm-family required-result weighted focus lock mismatch. actual=[{string.Join(" | ", algorithmFamilyRequiredResultPrimaryFieldFocuses.Take(6).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:{x.WeightedCount}:{x.WeightedShare:P1}:{string.Join("/", x.CanonicalCodes)}:{string.Join("/", x.AlgorithmFamilies)}:{x.Summary}"))}]");
        }

        foreach (var row in expected)
        {
            var snapshot = actual.FirstOrDefault(x => string.Equals(x.CanonicalCode, row.CanonicalCode, StringComparison.Ordinal));
            if (snapshot is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing row for {row.CanonicalCode}.");
            }

            if (snapshot.TotalCount != row.TotalCount
                || snapshot.BaselineCount != row.BaselineCount
                || snapshot.DominantCount != row.DominantCount
                || snapshot.SelectedCount != row.SelectedCount)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: numeric mismatch for {row.CanonicalCode}.");
            }

            AssertRoute(snapshot.BaselineRoute, row.CanonicalCode, row.BaselineMethod, $"baseline/{row.CanonicalCode}");
            AssertRoute(snapshot.DominantRoute, row.CanonicalCode, row.DominantMethod, $"dominant/{row.CanonicalCode}");
            AssertRoute(snapshot.SelectedRoute, row.CanonicalCode, row.SelectedMethod, $"selected/{row.CanonicalCode}");

            if (!string.Equals(snapshot.SelectionStrategy, row.SelectionStrategy, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selection strategy mismatch for {row.CanonicalCode}. expected={row.SelectionStrategy}, actual={snapshot.SelectionStrategy}");
            }

            if (snapshot.ShouldUseDominantRoute != row.ShouldUseDominantRoute)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: dominant-route flag mismatch for {row.CanonicalCode}.");
            }

            if (Math.Abs(snapshot.BaselineShare - row.BaselineShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: baseline share mismatch for {row.CanonicalCode}. expected={row.BaselineShare}, actual={snapshot.BaselineShare}");
            }

            if (Math.Abs(snapshot.DominantShare - row.DominantShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: dominant share mismatch for {row.CanonicalCode}. expected={row.DominantShare}, actual={snapshot.DominantShare}");
            }

            if (Math.Abs(snapshot.SelectedShare - row.SelectedShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected share mismatch for {row.CanonicalCode}. expected={row.SelectedShare}, actual={snapshot.SelectedShare}");
            }

            if (Math.Abs(snapshot.DominantOverrideThreshold - DominantOverrideThreshold) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: override threshold mismatch for {row.CanonicalCode}. expected={DominantOverrideThreshold}, actual={snapshot.DominantOverrideThreshold}");
            }

            var selectedRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(row.CanonicalCode, row.SelectedMethod)
                ?? throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected route missing for {row.CanonicalCode}:{row.SelectedMethod}.");
            if (!string.Equals(snapshot.AlgorithmEntry, selectedRoute.LegacyAlgorithmEntry, StringComparison.Ordinal)
                || !string.Equals(snapshot.SettingsMethodName, selectedRoute.LegacySettingsMethodName, StringComparison.Ordinal)
                || !string.Equals(snapshot.LegacyMethodName, selectedRoute.LegacyMethodName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected route metadata mismatch for {row.CanonicalCode}.");
            }

            if (snapshot.DominantLeadCount != row.DominantLeadCount
                || snapshot.DominantLeadPercentagePoints != row.DominantLeadPercentagePoints)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: lead summary mismatch for {row.CanonicalCode}. expectedCount={row.DominantLeadCount}, actualCount={snapshot.DominantLeadCount}, expectedPp={row.DominantLeadPercentagePoints}, actualPp={snapshot.DominantLeadPercentagePoints}");
            }

            if (Math.Abs(snapshot.SelectedLeadCountVsBaseline - row.SelectedLeadCountVsBaseline) > 0.0001d
                || snapshot.SelectedLeadPercentagePointsVsBaseline != row.SelectedLeadPercentagePointsVsBaseline)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected-vs-baseline lead mismatch for {row.CanonicalCode}. expectedCount={row.SelectedLeadCountVsBaseline}, actualCount={snapshot.SelectedLeadCountVsBaseline}, expectedPp={row.SelectedLeadPercentagePointsVsBaseline}, actualPp={snapshot.SelectedLeadPercentagePointsVsBaseline}");
            }

            if (!string.Equals(snapshot.SelectionReason, row.SelectionReason, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selection reason mismatch for {row.CanonicalCode}. expected='{row.SelectionReason}', actual='{snapshot.SelectionReason}'");
            }

            if (!string.Equals(snapshot.SelectedMethodSummary, row.SelectedMethodSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected method summary mismatch for {row.CanonicalCode}. expected='{row.SelectedMethodSummary}', actual='{snapshot.SelectedMethodSummary}'");
            }

            if (!string.Equals(snapshot.BaselineDominantComparisonSummary, row.BaselineDominantComparisonSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: baseline/dominant comparison summary mismatch for {row.CanonicalCode}. expected='{row.BaselineDominantComparisonSummary}', actual='{snapshot.BaselineDominantComparisonSummary}'");
            }

            if (snapshot.Distributions.Count != row.Distributions.Count)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: distribution count mismatch for {row.CanonicalCode}.");
            }

            AssertLegacyCodeSelection(snapshot, connection, row.CanonicalCode);
            AssertLegacyCodeDistributionConsistency(snapshot, legacyCodeDistributionRows, row.CanonicalCode);
            AssertSampleCountReadiness(snapshot);
            AssertDecisionAnchorPriorityDistribution(snapshot);
            AssertDecisionAnchorPrimaryFieldDistribution(snapshot);
            AssertLegacyAlgorithmSourceEvidence(snapshot);

            foreach (var distribution in row.Distributions)
            {
                var actualDistribution = snapshot.Distributions.FirstOrDefault(x => x.MethodValue == distribution.MethodValue);
                if (actualDistribution is null)
                {
                    throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing distribution {row.CanonicalCode}:{distribution.MethodValue}.");
                }

                if (actualDistribution.Count != distribution.Count)
                {
                    throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: distribution count mismatch for {row.CanonicalCode}:{distribution.MethodValue}.");
                }

                if (Math.Abs(actualDistribution.Share - distribution.Share) > 0.0001d)
                {
                    throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: distribution share mismatch for {row.CanonicalCode}:{distribution.MethodValue}. expected={distribution.Share}, actual={actualDistribution.Share}");
                }

                AssertRoute(actualDistribution.Route, row.CanonicalCode, distribution.MethodValue, $"distribution/{row.CanonicalCode}:{distribution.MethodValue}");
            }
        }
    }

    private static void AssertCrossPlanDecisionAnchorPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        var expected = MotorYPrimaryFieldFocusFactory.BuildCrossPlanDecisionAnchorPrimaryFieldFocuses(plans);

        if (focuses.Count != expected.Count)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: cross-plan primary-field focus count mismatch. expected={expected.Count}, actual={focuses.Count}");
        }

        foreach (var row in expected)
        {
            var actual = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, row.PrimaryField, StringComparison.Ordinal));
            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing cross-plan primary-field focus {row.PrimaryField}.");
            }

            if (actual.Count != row.Count
                || Math.Abs(actual.Share - row.Share) > 0.0001d
                || actual.WeightedCount != row.WeightedCount
                || Math.Abs(actual.WeightedShare - row.WeightedShare) > 0.0001d
                || actual.BaselineCount != row.BaselineCount
                || Math.Abs(actual.BaselineShare - row.BaselineShare) > 0.0001d
                || actual.BaselineWeightedCount != row.BaselineWeightedCount
                || Math.Abs(actual.BaselineWeightedShare - row.BaselineWeightedShare) > 0.0001d
                || actual.DominantCount != row.DominantCount
                || Math.Abs(actual.DominantShare - row.DominantShare) > 0.0001d
                || actual.DominantWeightedCount != row.DominantWeightedCount
                || Math.Abs(actual.DominantWeightedShare - row.DominantWeightedShare) > 0.0001d
                || actual.SelectedCount != row.SelectedCount
                || Math.Abs(actual.SelectedShare - row.SelectedShare) > 0.0001d
                || actual.SelectedWeightedCount != row.SelectedWeightedCount
                || Math.Abs(actual.SelectedWeightedShare - row.SelectedWeightedShare) > 0.0001d
                || actual.BaselineMethodValue != row.BaselineMethodValue
                || !string.Equals(actual.BaselineMethodKey, row.BaselineMethodKey, StringComparison.Ordinal)
                || !string.Equals(actual.BaselineProfileKey, row.BaselineProfileKey, StringComparison.Ordinal)
                || actual.DominantMethodValue != row.DominantMethodValue
                || !string.Equals(actual.DominantMethodKey, row.DominantMethodKey, StringComparison.Ordinal)
                || !string.Equals(actual.DominantProfileKey, row.DominantProfileKey, StringComparison.Ordinal)
                || actual.SelectedMethodValue != row.SelectedMethodValue
                || !string.Equals(actual.SelectedMethodKey, row.SelectedMethodKey, StringComparison.Ordinal)
                || !string.Equals(actual.SelectedProfileKey, row.SelectedProfileKey, StringComparison.Ordinal)
                || !actual.CanonicalCodes.SequenceEqual(row.CanonicalCodes, StringComparer.Ordinal)
                || !actual.MethodValues.SequenceEqual(row.MethodValues)
                || !actual.MethodKeys.SequenceEqual(row.MethodKeys, StringComparer.Ordinal)
                || !actual.ProfileKeys.SequenceEqual(row.ProfileKeys, StringComparer.Ordinal)
                || !actual.LegacyMethodNames.SequenceEqual(row.LegacyMethodNames, StringComparer.Ordinal)
                || !actual.SettingsMethodNames.SequenceEqual(row.SettingsMethodNames, StringComparer.Ordinal)
                || !actual.AnchorKeys.SequenceEqual(row.AnchorKeys, StringComparer.Ordinal)
                || !actual.SuggestedNextStepFocuses.SequenceEqual(row.SuggestedNextStepFocuses, StringComparer.Ordinal)
                || !actual.SuggestedNextStepPriorities.SequenceEqual(row.SuggestedNextStepPriorities, StringComparer.Ordinal)
                || !string.Equals(actual.Summary, row.Summary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: cross-plan primary-field focus mismatch for {row.PrimaryField}. expected={row.Count}/{row.Share}:{row.WeightedCount}/{row.WeightedShare}:baseline={row.BaselineCount}/{row.BaselineShare}:{row.BaselineWeightedCount}/{row.BaselineWeightedShare}:dominant={row.DominantCount}/{row.DominantShare}:{row.DominantWeightedCount}/{row.DominantWeightedShare}:selected={row.SelectedCount}/{row.SelectedShare}:{row.SelectedWeightedCount}/{row.SelectedWeightedShare}:baseline-route={row.BaselineMethodValue}/{row.BaselineMethodKey}/{row.BaselineProfileKey}:dominant-route={row.DominantMethodValue}/{row.DominantMethodKey}/{row.DominantProfileKey}:selected-route={row.SelectedMethodValue}/{row.SelectedMethodKey}/{row.SelectedProfileKey}:methods={string.Join(',', row.MethodValues)}:methodKeys={string.Join(',', row.MethodKeys)}:profiles={string.Join(',', row.ProfileKeys)}:legacy={string.Join(',', row.LegacyMethodNames)}:settings={string.Join(',', row.SettingsMethodNames)}:{string.Join(',', row.CanonicalCodes)}:'{row.Summary}', actual={actual.Count}/{actual.Share}:{actual.WeightedCount}/{actual.WeightedShare}:baseline={actual.BaselineCount}/{actual.BaselineShare}:{actual.BaselineWeightedCount}/{actual.BaselineWeightedShare}:dominant={actual.DominantCount}/{actual.DominantShare}:{actual.DominantWeightedCount}/{actual.DominantWeightedShare}:selected={actual.SelectedCount}/{actual.SelectedShare}:{actual.SelectedWeightedCount}/{actual.SelectedWeightedShare}:baseline-route={actual.BaselineMethodValue}/{actual.BaselineMethodKey}/{actual.BaselineProfileKey}:dominant-route={actual.DominantMethodValue}/{actual.DominantMethodKey}/{actual.DominantProfileKey}:selected-route={actual.SelectedMethodValue}/{actual.SelectedMethodKey}/{actual.SelectedProfileKey}:methods={string.Join(',', actual.MethodValues)}:methodKeys={string.Join(',', actual.MethodKeys)}:profiles={string.Join(',', actual.ProfileKeys)}:legacy={string.Join(',', actual.LegacyMethodNames)}:settings={string.Join(',', actual.SettingsMethodNames)}:{string.Join(',', actual.CanonicalCodes)}:'{actual.Summary}'");
            }
        }
    }

    private static void AssertLegacyAlgorithmSourceEvidence(MotorYMethodAdaptationPlanSnapshot snapshot)
    {
        var sourceEvidences = snapshot.SourceEvidences;
        if (sourceEvidences.Count == 0)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: no source evidences for {snapshot.CanonicalCode}.");
        }

        if (string.Equals(snapshot.CanonicalCode, MotorYTestMethodCodes.HeatRun, StringComparison.Ordinal))
        {
            var thetaBWindow = sourceEvidences.FirstOrDefault(x => string.Equals(x.SectionKey, "theta-b-last-quarter-window", StringComparison.Ordinal));
            if (thetaBWindow is null
                || thetaBWindow.StartLine != 563
                || thetaBWindow.EndLine != 569
                || !thetaBWindow.ReferencedFields.Contains("Data1List.Time", StringComparer.Ordinal)
                || !thetaBWindow.ReferencedFields.Contains("Data1List.θb", StringComparer.Ordinal)
                || !thetaBWindow.ReferencedFields.Contains("θb", StringComparer.Ordinal)
                || !thetaBWindow.Summary.Contains("1/4", StringComparison.Ordinal)
                || !thetaBWindow.Summary.Contains("环境温度", StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: HeatRun theta-b evidence mismatch. actual='{thetaBWindow?.Summary}', lines={thetaBWindow?.StartLine}-{thetaBWindow?.EndLine}, fields=[{string.Join(",", thetaBWindow?.ReferencedFields ?? Array.Empty<string>())}]");
            }

            var rnSelection = sourceEvidences.FirstOrDefault(x => string.Equals(x.SectionKey, "rn-selection", StringComparison.Ordinal));
            if (rnSelection is null
                || rnSelection.StartLine != 570
                || rnSelection.EndLine != 605
                || !rnSelection.ReferencedFields.Contains("Data2List.Time", StringComparer.Ordinal)
                || !rnSelection.ReferencedFields.Contains("Data2List.R", StringComparer.Ordinal)
                || !rnSelection.ReferencedFields.Contains("Rw", StringComparer.Ordinal)
                || !rnSelection.ReferencedFields.Contains("Rn", StringComparer.Ordinal)
                || !rnSelection.ReferencedFields.Contains("HotStateType", StringComparer.Ordinal)
                || rnSelection.ReferencedFields.Contains("Data1List.θb", StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: HeatRun rn-selection evidence mismatch. actual='{rnSelection?.Summary}', lines={rnSelection?.StartLine}-{rnSelection?.EndLine}, fields=[{string.Join(",", rnSelection?.ReferencedFields ?? Array.Empty<string>())}]");
            }
        }
    }

    private static void AssertCrossPlanDecisionAnchorPrimaryFieldFocusSummaries(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        var totalPlans = plans.Count;
        var totalWeighted = plans.Sum(plan => Math.Max(1, plan.SelectedCount));
        if (totalPlans == 0 || totalWeighted == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: cross-plan decision-anchor primary-field baseline is empty.");
        }

        var gb = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "GB", StringComparison.Ordinal));
        var coefficientOfPfe = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "CoefficientOfPfe", StringComparison.Ordinal));
        var pfw = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));

        var expectedWeightedCountGb = plans
            .Where(plan => string.Equals(plan.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal)
                || string.Equals(plan.CanonicalCode, MotorYTestMethodCodes.HeatRun, StringComparison.Ordinal))
            .Sum(plan => Math.Max(1, plan.SelectedCount));
        var expectedWeightedShareGb = Math.Round((double)expectedWeightedCountGb / totalWeighted, 4, MidpointRounding.AwayFromZero);

        var expectedWeightedCountNoLoad = plans
            .Where(plan => string.Equals(plan.CanonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal))
            .Sum(plan => Math.Max(1, plan.SelectedCount));
        var expectedWeightedShareNoLoad = Math.Round((double)expectedWeightedCountNoLoad / totalWeighted, 4, MidpointRounding.AwayFromZero);

        var expectedSummary = $"cross-plan decision-anchor primary fields top 3/{focuses.Count}: GB=2 (33pp, weighted {(int)Math.Round(expectedWeightedShareGb * 100d, MidpointRounding.AwayFromZero)}pp); CoefficientOfPfe=1 (17pp, weighted {(int)Math.Round(expectedWeightedShareNoLoad * 100d, MidpointRounding.AwayFromZero)}pp); Pfw=1 (17pp, weighted {(int)Math.Round(expectedWeightedShareNoLoad * 100d, MidpointRounding.AwayFromZero)}pp)";

        if (gb is null
            || gb.Count != 2
            || Math.Abs(gb.Share - 0.3333d) > 0.0001d
            || gb.WeightedCount != expectedWeightedCountGb
            || Math.Abs(gb.WeightedShare - expectedWeightedShareGb) > 0.0001d
            || !gb.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.HeatRun, MotorYTestMethodCodes.LoadB }, StringComparer.Ordinal)
            || !gb.AnchorKeys.SequenceEqual(new[] { "gb-ratios-branch", "gb-temperature-branch" }, StringComparer.Ordinal)
            || !gb.SuggestedNextStepFocuses.SequenceEqual(new[] { "额定参数", "负载B分支", "热态分支" }, StringComparer.Ordinal)
            || !gb.SuggestedNextStepPriorities.SequenceEqual(new[] { "blocking" }, StringComparer.Ordinal)
            || coefficientOfPfe is null
            || coefficientOfPfe.Count != 1
            || Math.Abs(coefficientOfPfe.Share - 0.1667d) > 0.0001d
            || coefficientOfPfe.WeightedCount != expectedWeightedCountNoLoad
            || Math.Abs(coefficientOfPfe.WeightedShare - expectedWeightedShareNoLoad) > 0.0001d
            || !coefficientOfPfe.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal)
            || !coefficientOfPfe.AnchorKeys.SequenceEqual(new[] { "pfw-split" }, StringComparer.Ordinal)
            || !coefficientOfPfe.SuggestedNextStepFocuses.SequenceEqual(new[] { "结果字段" }, StringComparer.Ordinal)
            || !coefficientOfPfe.SuggestedNextStepPriorities.SequenceEqual(new[] { "blocking" }, StringComparer.Ordinal)
            || pfw is null
            || pfw.Count != 1
            || Math.Abs(pfw.Share - 0.1667d) > 0.0001d
            || pfw.WeightedCount != expectedWeightedCountNoLoad
            || Math.Abs(pfw.WeightedShare - expectedWeightedShareNoLoad) > 0.0001d
            || !pfw.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal)
            || !pfw.AnchorKeys.SequenceEqual(new[] { "pfw-fit-window" }, StringComparer.Ordinal)
            || !pfw.SuggestedNextStepFocuses.SequenceEqual(new[] { "结果字段" }, StringComparer.Ordinal)
            || !pfw.SuggestedNextStepPriorities.SequenceEqual(new[] { "follow-up" }, StringComparer.Ordinal)
            || !plans.All(plan => string.Equals(plan.CrossPlanDecisionAnchorPrimaryFieldSummary, expectedSummary, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: explicit cross-plan decision-anchor primary-field summary mismatch. expectedSummary='{expectedSummary}', actualSummary='{plans.FirstOrDefault()?.CrossPlanDecisionAnchorPrimaryFieldSummary}', actual=[{string.Join(" | ", focuses.Take(6).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:{x.WeightedCount}:{x.WeightedShare:P1}:{string.Join("/", x.CanonicalCodes)}:{string.Join("/", x.AnchorKeys)}:{string.Join("/", x.SuggestedNextStepPriorities)}:{string.Join("/", x.SuggestedNextStepFocuses)}"))}]");
        }
    }

    private static void AssertCrossPlanRequiredResultPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        var expected = MotorYPrimaryFieldFocusFactory.BuildCrossPlanRequiredResultPrimaryFieldFocuses(plans);

        if (focuses.Count != expected.Count)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: cross-plan required-result primary-field focus count mismatch. expected={expected.Count}, actual={focuses.Count}");
        }

        foreach (var row in expected)
        {
            var actual = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, row.PrimaryField, StringComparison.Ordinal));
            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing cross-plan required-result primary-field focus {row.PrimaryField}.");
            }

            if (actual.Count != row.Count
                || Math.Abs(actual.Share - row.Share) > 0.0001d
                || actual.WeightedCount != row.WeightedCount
                || Math.Abs(actual.WeightedShare - row.WeightedShare) > 0.0001d
                || !actual.CanonicalCodes.SequenceEqual(row.CanonicalCodes, StringComparer.Ordinal)
                || !actual.AnchorKeys.SequenceEqual(row.AnchorKeys, StringComparer.Ordinal)
                || !actual.SuggestedNextStepFocuses.SequenceEqual(row.SuggestedNextStepFocuses, StringComparer.Ordinal)
                || !actual.SuggestedNextStepPriorities.SequenceEqual(row.SuggestedNextStepPriorities, StringComparer.Ordinal)
                || !string.Equals(actual.Summary, row.Summary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: cross-plan required-result primary-field focus mismatch for {row.PrimaryField}. expected={row.Count}/{row.Share}:{row.WeightedCount}/{row.WeightedShare}:{string.Join(',', row.CanonicalCodes)}:'{row.Summary}', actual={actual.Count}/{actual.Share}:{actual.WeightedCount}/{actual.WeightedShare}:{string.Join(',', actual.CanonicalCodes)}:'{actual.Summary}'");
            }
        }
    }

    private static void AssertAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        var expected = MotorYPrimaryFieldFocusFactory.BuildAlgorithmFamilyDecisionAnchorPrimaryFieldFocuses(plans);

        if (focuses.Count != expected.Count)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: algorithm-family decision-anchor primary-field focus count mismatch. expected={expected.Count}, actual={focuses.Count}");
        }

        foreach (var row in expected)
        {
            var actual = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, row.PrimaryField, StringComparison.Ordinal)
                && x.AlgorithmFamilies.SequenceEqual(row.AlgorithmFamilies, StringComparer.Ordinal));
            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing algorithm-family decision-anchor primary-field focus {row.PrimaryField}/{string.Join('/', row.AlgorithmFamilies)}.");
            }

            if (actual.Count != row.Count
                || Math.Abs(actual.Share - row.Share) > 0.0001d
                || actual.WeightedCount != row.WeightedCount
                || Math.Abs(actual.WeightedShare - row.WeightedShare) > 0.0001d
                || actual.BaselineCount != row.BaselineCount
                || Math.Abs(actual.BaselineShare - row.BaselineShare) > 0.0001d
                || actual.BaselineWeightedCount != row.BaselineWeightedCount
                || Math.Abs(actual.BaselineWeightedShare - row.BaselineWeightedShare) > 0.0001d
                || actual.DominantCount != row.DominantCount
                || Math.Abs(actual.DominantShare - row.DominantShare) > 0.0001d
                || actual.DominantWeightedCount != row.DominantWeightedCount
                || Math.Abs(actual.DominantWeightedShare - row.DominantWeightedShare) > 0.0001d
                || actual.SelectedCount != row.SelectedCount
                || Math.Abs(actual.SelectedShare - row.SelectedShare) > 0.0001d
                || actual.SelectedWeightedCount != row.SelectedWeightedCount
                || Math.Abs(actual.SelectedWeightedShare - row.SelectedWeightedShare) > 0.0001d
                || actual.BaselineMethodValue != row.BaselineMethodValue
                || !string.Equals(actual.BaselineMethodKey, row.BaselineMethodKey, StringComparison.Ordinal)
                || !string.Equals(actual.BaselineProfileKey, row.BaselineProfileKey, StringComparison.Ordinal)
                || actual.DominantMethodValue != row.DominantMethodValue
                || !string.Equals(actual.DominantMethodKey, row.DominantMethodKey, StringComparison.Ordinal)
                || !string.Equals(actual.DominantProfileKey, row.DominantProfileKey, StringComparison.Ordinal)
                || actual.SelectedMethodValue != row.SelectedMethodValue
                || !string.Equals(actual.SelectedMethodKey, row.SelectedMethodKey, StringComparison.Ordinal)
                || !string.Equals(actual.SelectedProfileKey, row.SelectedProfileKey, StringComparison.Ordinal)
                || !actual.CanonicalCodes.SequenceEqual(row.CanonicalCodes, StringComparer.Ordinal)
                || !actual.MethodValues.SequenceEqual(row.MethodValues)
                || !actual.MethodKeys.SequenceEqual(row.MethodKeys, StringComparer.Ordinal)
                || !actual.ProfileKeys.SequenceEqual(row.ProfileKeys, StringComparer.Ordinal)
                || !actual.LegacyMethodNames.SequenceEqual(row.LegacyMethodNames, StringComparer.Ordinal)
                || !actual.SettingsMethodNames.SequenceEqual(row.SettingsMethodNames, StringComparer.Ordinal)
                || !actual.AnchorKeys.SequenceEqual(row.AnchorKeys, StringComparer.Ordinal)
                || !actual.SuggestedNextStepFocuses.SequenceEqual(row.SuggestedNextStepFocuses, StringComparer.Ordinal)
                || !actual.SuggestedNextStepPriorities.SequenceEqual(row.SuggestedNextStepPriorities, StringComparer.Ordinal)
                || !string.Equals(actual.Summary, row.Summary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: algorithm-family decision-anchor primary-field focus mismatch for {row.PrimaryField}/{string.Join('/', row.AlgorithmFamilies)}. expected={row.Count}/{row.Share}:{row.WeightedCount}/{row.WeightedShare}:baseline={row.BaselineCount}/{row.BaselineShare}:{row.BaselineWeightedCount}/{row.BaselineWeightedShare}:dominant={row.DominantCount}/{row.DominantShare}:{row.DominantWeightedCount}/{row.DominantWeightedShare}:selected={row.SelectedCount}/{row.SelectedShare}:{row.SelectedWeightedCount}/{row.SelectedWeightedShare}:baseline-route={row.BaselineMethodValue}/{row.BaselineMethodKey}/{row.BaselineProfileKey}:dominant-route={row.DominantMethodValue}/{row.DominantMethodKey}/{row.DominantProfileKey}:selected-route={row.SelectedMethodValue}/{row.SelectedMethodKey}/{row.SelectedProfileKey}:methods={string.Join(',', row.MethodValues)}:methodKeys={string.Join(',', row.MethodKeys)}:profiles={string.Join(',', row.ProfileKeys)}:legacy={string.Join(',', row.LegacyMethodNames)}:settings={string.Join(',', row.SettingsMethodNames)}:{string.Join(',', row.CanonicalCodes)}:'{row.Summary}', actual={actual.Count}/{actual.Share}:{actual.WeightedCount}/{actual.WeightedShare}:baseline={actual.BaselineCount}/{actual.BaselineShare}:{actual.BaselineWeightedCount}/{actual.BaselineWeightedShare}:dominant={actual.DominantCount}/{actual.DominantShare}:{actual.DominantWeightedCount}/{actual.DominantWeightedShare}:selected={actual.SelectedCount}/{actual.SelectedShare}:{actual.SelectedWeightedCount}/{actual.SelectedWeightedShare}:baseline-route={actual.BaselineMethodValue}/{actual.BaselineMethodKey}/{actual.BaselineProfileKey}:dominant-route={actual.DominantMethodValue}/{actual.DominantMethodKey}/{actual.DominantProfileKey}:selected-route={actual.SelectedMethodValue}/{actual.SelectedMethodKey}/{actual.SelectedProfileKey}:methods={string.Join(',', actual.MethodValues)}:methodKeys={string.Join(',', actual.MethodKeys)}:profiles={string.Join(',', actual.ProfileKeys)}:legacy={string.Join(',', actual.LegacyMethodNames)}:settings={string.Join(',', actual.SettingsMethodNames)}:{string.Join(',', actual.CanonicalCodes)}:'{actual.Summary}'");
            }
        }
    }

    private static void AssertAlgorithmFamilyRequiredResultPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        var expected = MotorYPrimaryFieldFocusFactory.BuildAlgorithmFamilyRequiredResultPrimaryFieldFocuses(plans);

        if (focuses.Count != expected.Count)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: algorithm-family required-result primary-field focus count mismatch. expected={expected.Count}, actual={focuses.Count}");
        }

        foreach (var row in expected)
        {
            var actual = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, row.PrimaryField, StringComparison.Ordinal)
                && x.AlgorithmFamilies.SequenceEqual(row.AlgorithmFamilies, StringComparer.Ordinal));
            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing algorithm-family required-result primary-field focus {row.PrimaryField}/{string.Join('/', row.AlgorithmFamilies)}.");
            }

            if (actual.Count != row.Count
                || Math.Abs(actual.Share - row.Share) > 0.0001d
                || actual.WeightedCount != row.WeightedCount
                || Math.Abs(actual.WeightedShare - row.WeightedShare) > 0.0001d
                || !actual.CanonicalCodes.SequenceEqual(row.CanonicalCodes, StringComparer.Ordinal)
                || !actual.AnchorKeys.SequenceEqual(row.AnchorKeys, StringComparer.Ordinal)
                || !actual.SuggestedNextStepFocuses.SequenceEqual(row.SuggestedNextStepFocuses, StringComparer.Ordinal)
                || !actual.SuggestedNextStepPriorities.SequenceEqual(row.SuggestedNextStepPriorities, StringComparer.Ordinal)
                || !string.Equals(actual.Summary, row.Summary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: algorithm-family required-result primary-field focus mismatch for {row.PrimaryField}/{string.Join('/', row.AlgorithmFamilies)}. expected={row.Count}/{row.Share}:{row.WeightedCount}/{row.WeightedShare}:{string.Join(',', row.CanonicalCodes)}:'{row.Summary}', actual={actual.Count}/{actual.Share}:{actual.WeightedCount}/{actual.WeightedShare}:{string.Join(',', actual.CanonicalCodes)}:'{actual.Summary}'");
            }
        }
    }

    private static void AssertAlgorithmFamilyDecisionAnchorPrimaryFieldFocusSummaries(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        var noLoadPlan = plans.FirstOrDefault(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal));
        var loadBPlan = plans.FirstOrDefault(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal));
        if (noLoadPlan is null || loadBPlan is null)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: missing NoLoad/LoadB plans for algorithm-family decision-anchor summary assertions.");
        }

        var gb = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "GB", StringComparison.Ordinal)
            && x.AlgorithmFamilies.SequenceEqual(new[] { MotorYTestMethodCodes.LoadB }, StringComparer.Ordinal));
        var coefficientOfPfe = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "CoefficientOfPfe", StringComparison.Ordinal)
            && x.AlgorithmFamilies.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal));
        var pfw = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal)
            && x.AlgorithmFamilies.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal));
        const string expectedSummary = "algorithm-family decision-anchor primary fields top 3/6: GB=1 (50pp, weighted 32pp, families LoadB); CoefficientOfPfe=1 (100pp, weighted 100pp, families NoLoad); Pfw=1 (100pp, weighted 100pp, families NoLoad)";

        if (gb is null
            || gb.Count != 1
            || Math.Abs(gb.Share - 0.5d) > 0.0001d
            || gb.WeightedCount != 61
            || Math.Abs(gb.WeightedShare - 0.3211d) > 0.0001d
            || !gb.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.LoadB }, StringComparer.Ordinal)
            || !gb.AnchorKeys.SequenceEqual(new[] { "gb-temperature-branch" }, StringComparer.Ordinal)
            || !gb.SuggestedNextStepFocuses.SequenceEqual(new[] { "热态分支" }, StringComparer.Ordinal)
            || !gb.SuggestedNextStepPriorities.SequenceEqual(new[] { "blocking" }, StringComparer.Ordinal)
            || coefficientOfPfe is null
            || coefficientOfPfe.Count != 1
            || Math.Abs(coefficientOfPfe.Share - 1d) > 0.0001d
            || coefficientOfPfe.WeightedCount != 431
            || Math.Abs(coefficientOfPfe.WeightedShare - 1d) > 0.0001d
            || !coefficientOfPfe.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal)
            || !coefficientOfPfe.AnchorKeys.SequenceEqual(new[] { "pfw-split" }, StringComparer.Ordinal)
            || !coefficientOfPfe.SuggestedNextStepFocuses.SequenceEqual(new[] { "结果字段" }, StringComparer.Ordinal)
            || !coefficientOfPfe.SuggestedNextStepPriorities.SequenceEqual(new[] { "blocking" }, StringComparer.Ordinal)
            || pfw is null
            || pfw.Count != 1
            || Math.Abs(pfw.Share - 1d) > 0.0001d
            || pfw.WeightedCount != 431
            || Math.Abs(pfw.WeightedShare - 1d) > 0.0001d
            || !pfw.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal)
            || !pfw.AnchorKeys.SequenceEqual(new[] { "pfw-fit-window" }, StringComparer.Ordinal)
            || !pfw.SuggestedNextStepFocuses.SequenceEqual(new[] { "结果字段" }, StringComparer.Ordinal)
            || !pfw.SuggestedNextStepPriorities.SequenceEqual(new[] { "follow-up" }, StringComparer.Ordinal)
            || !string.Equals(noLoadPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary, expectedSummary, StringComparison.Ordinal)
            || !string.Equals(loadBPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary, expectedSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: explicit algorithm-family decision-anchor primary-field summary mismatch. expectedSummary='{expectedSummary}', noLoadSummary='{noLoadPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary}', loadBSummary='{loadBPlan.AlgorithmFamilyDecisionAnchorPrimaryFieldSummary}', actual=[{string.Join(" | ", focuses.Take(6).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:{x.WeightedCount}:{x.WeightedShare:P1}:{string.Join("/", x.AlgorithmFamilies)}:{string.Join("/", x.CanonicalCodes)}:{string.Join("/", x.AnchorKeys)}:{string.Join("/", x.SuggestedNextStepPriorities)}:{string.Join("/", x.SuggestedNextStepFocuses)}"))}]");
        }
    }

    private static void AssertVariantKindDecisionAnchorPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> actual)
    {
        var expected = MotorYPrimaryFieldFocusFactory.BuildVariantKindDecisionAnchorPrimaryFieldFocuses(plans);
        AssertPrimaryFieldFocusesEquivalent(expected, actual, "variant-kind decision-anchor primary-field focuses");
    }

    private static void AssertVariantKindDecisionAnchorPrimaryFieldFocusSummaries(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> actual)
    {
        var expectedSummary = MotorYPrimaryFieldFocusFactory.BuildVariantKindFocusSummary("decision-anchor", actual);
        foreach (var plan in plans)
        {
            if (!string.Equals(plan.VariantKindDecisionAnchorPrimaryFieldSummary, expectedSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: variant-kind decision-anchor summary mismatch for {plan.CanonicalCode}. expected='{expectedSummary}', actual='{plan.VariantKindDecisionAnchorPrimaryFieldSummary}'.");
            }
        }
    }

    private static void AssertVariantKindRequiredResultPrimaryFieldFocuses(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> actual)
    {
        var expected = MotorYPrimaryFieldFocusFactory.BuildVariantKindRequiredResultPrimaryFieldFocuses(plans);
        AssertPrimaryFieldFocusesEquivalent(expected, actual, "variant-kind required-result primary-field focuses");
    }

    private static void AssertVariantKindRequiredResultPrimaryFieldFocusSummaries(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> actual)
    {
        var expectedSummary = MotorYPrimaryFieldFocusFactory.BuildVariantKindFocusSummary("required-result", actual);
        foreach (var plan in plans)
        {
            if (!string.Equals(plan.VariantKindRequiredResultPrimaryFieldSummary, expectedSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: variant-kind required-result summary mismatch for {plan.CanonicalCode}. expected='{expectedSummary}', actual='{plan.VariantKindRequiredResultPrimaryFieldSummary}'.");
            }
        }
    }

    private static void AssertAlgorithmFamilyRequiredResultPrimaryFieldFocusSummaries(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        var noLoadPlan = plans.FirstOrDefault(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal));
        var loadBPlan = plans.FirstOrDefault(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal));
        if (noLoadPlan is null || loadBPlan is null)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: missing NoLoad/LoadB plans for algorithm-family required-result summary assertions.");
        }

        var coefficientOfPfe = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "CoefficientOfPfe", StringComparison.Ordinal)
            && x.AlgorithmFamilies.SequenceEqual(new[] { MotorYTestMethodCodes.LoadB }, StringComparer.Ordinal));
        var coefficientOfPfeNoLoad = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "CoefficientOfPfe", StringComparison.Ordinal)
            && x.AlgorithmFamilies.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal));
        var pfwLoadB = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal)
            && x.AlgorithmFamilies.SequenceEqual(new[] { MotorYTestMethodCodes.LoadB }, StringComparer.Ordinal));
        var pcu2 = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "Pcu2", StringComparison.Ordinal)
            && x.AlgorithmFamilies.SequenceEqual(new[] { MotorYTestMethodCodes.LoadB }, StringComparer.Ordinal));
        const string expectedSummary = "algorithm-family required-result primary fields top 3/18: CoefficientOfPfe=2 (100pp, weighted 100pp, families LoadB/NoLoad); Pfw=2 (100pp, weighted 100pp, families LoadB/NoLoad); Pcu2=1 (50pp, weighted 32pp, families LoadB)";

        if (coefficientOfPfe is null
            || coefficientOfPfe.Count != 1
            || Math.Abs(coefficientOfPfe.Share - 1d) > 0.0001d
            || coefficientOfPfe.WeightedCount != 61
            || Math.Abs(coefficientOfPfe.WeightedShare - 1d) > 0.0001d
            || !coefficientOfPfe.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.LoadB }, StringComparer.Ordinal)
            || !coefficientOfPfe.SuggestedNextStepFocuses.SequenceEqual(new[] { "结果字段" }, StringComparer.Ordinal)
            || !coefficientOfPfe.SuggestedNextStepPriorities.SequenceEqual(new[] { "result-fields" }, StringComparer.Ordinal)
            || coefficientOfPfeNoLoad is null
            || coefficientOfPfeNoLoad.Count != 1
            || Math.Abs(coefficientOfPfeNoLoad.Share - 1d) > 0.0001d
            || coefficientOfPfeNoLoad.WeightedCount != 431
            || Math.Abs(coefficientOfPfeNoLoad.WeightedShare - 1d) > 0.0001d
            || !coefficientOfPfeNoLoad.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal)
            || pfwLoadB is null
            || pfwLoadB.Count != 1
            || Math.Abs(pfwLoadB.Share - 1d) > 0.0001d
            || pfwLoadB.WeightedCount != 61
            || Math.Abs(pfwLoadB.WeightedShare - 1d) > 0.0001d
            || pcu2 is null
            || pcu2.Count != 1
            || Math.Abs(pcu2.Share - 0.5d) > 0.0001d
            || pcu2.WeightedCount != 61
            || Math.Abs(pcu2.WeightedShare - 0.3211d) > 0.0001d
            || !pcu2.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.LoadB }, StringComparer.Ordinal)
            || !pcu2.SuggestedNextStepFocuses.SequenceEqual(new[] { "结果字段" }, StringComparer.Ordinal)
            || !pcu2.SuggestedNextStepPriorities.SequenceEqual(new[] { "result-fields" }, StringComparer.Ordinal)
            || !string.Equals(noLoadPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary, expectedSummary, StringComparison.Ordinal)
            || !string.Equals(loadBPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary, expectedSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: explicit algorithm-family required-result primary-field summary mismatch. expectedSummary='{expectedSummary}', noLoadSummary='{noLoadPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary}', loadBSummary='{loadBPlan.AlgorithmFamilyRequiredResultPrimaryFieldSummary}', actual=[{string.Join(" | ", focuses.Take(8).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:{x.WeightedCount}:{x.WeightedShare:P1}:{string.Join("/", x.AlgorithmFamilies)}:{string.Join("/", x.CanonicalCodes)}:{string.Join("/", x.SuggestedNextStepPriorities)}:{string.Join("/", x.SuggestedNextStepFocuses)}"))}]");
        }
    }

    private static void AssertCrossPlanRequiredResultPrimaryFieldFocusSummaries(
        IReadOnlyList<MotorYMethodAdaptationPlanSnapshot> plans,
        IReadOnlyList<MotorYPrimaryFieldFocusSnapshot> focuses)
    {
        var totalPlans = plans.Count;
        var totalWeighted = plans.Sum(plan => Math.Max(1, plan.SelectedCount));
        if (totalPlans == 0 || totalWeighted == 0)
        {
            throw new InvalidOperationException("stp.db Motor_Y method adaptation plan smoke test failed: cross-plan required-result primary-field baseline is empty.");
        }

        var pfw = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "Pfw", StringComparison.Ordinal));
        var coefficientOfPfe = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "CoefficientOfPfe", StringComparison.Ordinal));
        var pcu2 = focuses.FirstOrDefault(x => string.Equals(x.PrimaryField, "Pcu2", StringComparison.Ordinal));

        var expectedWeightedCount = plans
            .Where(plan => string.Equals(plan.CanonicalCode, MotorYTestMethodCodes.NoLoad, StringComparison.Ordinal)
                || string.Equals(plan.CanonicalCode, MotorYTestMethodCodes.LoadA, StringComparison.Ordinal)
                || string.Equals(plan.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal))
            .Sum(plan => Math.Max(1, plan.SelectedCount));
        var expectedWeightedShare = Math.Round((double)expectedWeightedCount / totalWeighted, 4, MidpointRounding.AwayFromZero);
        var expectedSummary = $"cross-plan required-result primary fields top 3/{focuses.Count}: CoefficientOfPfe=3 (50pp, weighted {(int)Math.Round(expectedWeightedShare * 100d, MidpointRounding.AwayFromZero)}pp); Pfw=3 (50pp, weighted {(int)Math.Round(expectedWeightedShare * 100d, MidpointRounding.AwayFromZero)}pp); Pcu2=2 (33pp, weighted 20pp); dominant=CoefficientOfPfe@families=LoadA/LoadB/NoLoad@codes=MotorY.LoadA/MotorY.LoadB/MotorY.NoLoad@method-keys=LoadA:60/LoadB:51/NoLoad:0@legacy-methods=A法负载试验/B法负载试验/空载试验@settings-methods=A法负载试验/B法负载试验/空载试验@algo-entries=GetLoadAData/GetLoadBData/GetNoLoadData@forms=FrmMotor_Y_LoadA/FrmMotor_Y_LoadB/FrmMotor_Y_NoLoad@form-ranges=L194/L263/L451@baseline=1/3@dominant-share=3/3@selected-share=3/3@upstream=MotorY.DcResistance/MotorY.HeatRun/MotorY.NoLoad@upstream-hints=observed 0/1 required upstream codes | observed 2/2 required upstream codes | observed 1/2 required upstream codes";

        if (pfw is null
            || pfw.Count != 3
            || Math.Abs(pfw.Share - 0.5d) > 0.0001d
            || pfw.WeightedCount != expectedWeightedCount
            || Math.Abs(pfw.WeightedShare - expectedWeightedShare) > 0.0001d
            || !pfw.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.LoadA, MotorYTestMethodCodes.LoadB, MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal)
            || !pfw.SuggestedNextStepFocuses.SequenceEqual(new[] { "中间结果锚点", "结果字段" }, StringComparer.Ordinal)
            || !pfw.SuggestedNextStepPriorities.SequenceEqual(new[] { "intermediate-result-fields", "result-fields" }, StringComparer.Ordinal)
            || coefficientOfPfe is null
            || coefficientOfPfe.Count != 3
            || Math.Abs(coefficientOfPfe.Share - 0.5d) > 0.0001d
            || coefficientOfPfe.WeightedCount != expectedWeightedCount
            || Math.Abs(coefficientOfPfe.WeightedShare - expectedWeightedShare) > 0.0001d
            || !coefficientOfPfe.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.LoadA, MotorYTestMethodCodes.LoadB, MotorYTestMethodCodes.NoLoad }, StringComparer.Ordinal)
            || pcu2 is null
            || pcu2.Count != 2
            || Math.Abs(pcu2.Share - 0.3333d) > 0.0001d
            || pcu2.WeightedCount != 81
            || Math.Abs(pcu2.WeightedShare - 0.198d) > 0.0001d
            || !pcu2.CanonicalCodes.SequenceEqual(new[] { MotorYTestMethodCodes.LoadA, MotorYTestMethodCodes.LoadB }, StringComparer.Ordinal)
            || !plans.All(plan => string.Equals(plan.CrossPlanRequiredResultPrimaryFieldSummary, expectedSummary, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: explicit cross-plan required-result primary-field summary mismatch. expectedSummary='{expectedSummary}', actualSummary='{plans.FirstOrDefault()?.CrossPlanRequiredResultPrimaryFieldSummary}', actual=[{string.Join(" | ", focuses.Take(5).Select(x => $"{x.PrimaryField}:{x.Count}:{x.Share:P1}:{x.WeightedCount}:{x.WeightedShare:P1}:{string.Join("/", x.CanonicalCodes)}:{string.Join("/", x.SuggestedNextStepPriorities)}:{string.Join("/", x.SuggestedNextStepFocuses)}"))}]");
        }
    }

    private static void AssertDecisionAnchorPriorityDistribution(MotorYMethodAdaptationPlanSnapshot snapshot)
    {
        var expectedResolutions = snapshot.LegacyDecisionAnchorResolutions
            .Select(x => new MotorYDecisionAnchorResolution
            {
                AnchorKey = x.AnchorKey,
                ResolvedByObservedPayload = x.ResolvedByObservedPayload,
                PartiallyResolvedByObservedPayload = x.PartiallyResolvedByObservedPayload,
                RequiredPayloadFields = x.RequiredPayloadFields,
                ObservedPayloadFields = x.ObservedPayloadFields,
                MissingPayloadFields = x.MissingPayloadFields,
                CoverageRatio = x.CoverageRatio,
                CoveragePercentagePoints = x.CoveragePercentagePoints,
                ResolutionStage = x.ResolutionStage,
                SuggestedNextStepCategory = x.SuggestedNextStepCategory,
                SuggestedNextStepFocus = x.SuggestedNextStepFocus,
                SuggestedNextStepFields = x.SuggestedNextStepFields,
                SuggestedNextSteps = x.SuggestedNextSteps,
                SuggestedNextStepSummary = x.SuggestedNextStepSummary,
                SuggestedNextStepPriority = x.SuggestedNextStepPriority,
                SuggestedNextStepPrioritySummary = x.SuggestedNextStepPrioritySummary,
                SuggestedNextStepCoverageSummary = x.SuggestedNextStepCoverageSummary,
                SuggestedPrimaryNextField = x.SuggestedPrimaryNextField,
                SuggestedPrimaryNextFieldSummary = x.SuggestedPrimaryNextFieldSummary,
                Summary = x.Summary
            })
            .ToArray();
        var expectedDistributions = MotorYDecisionAnchorResolutionFactory.BuildPriorityDistributions(expectedResolutions);
        var expectedSummary = MotorYDecisionAnchorResolutionFactory.BuildPrioritySummary(expectedResolutions);
        var expectedSuggestedNextSteps = MotorYDecisionAnchorResolutionFactory.BuildSuggestedNextSteps(expectedResolutions);
        var expectedNextActionSummary = MotorYDecisionAnchorResolutionFactory.BuildNextActionSummary(expectedResolutions);
        var expectedGapPreviewSummary = MotorYDecisionAnchorResolutionFactory.BuildGapPreviewSummary(expectedResolutions);
        var expectedResolutionSummary = MotorYDecisionAnchorResolutionFactory.BuildSummary(expectedResolutions);

        if (snapshot.DecisionAnchorPriorityDistributions.Count != expectedDistributions.Count)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor priority distribution count mismatch for {snapshot.CanonicalCode}. expected={expectedDistributions.Count}, actual={snapshot.DecisionAnchorPriorityDistributions.Count}");
        }

        foreach (var expected in expectedDistributions)
        {
            var actual = snapshot.DecisionAnchorPriorityDistributions.FirstOrDefault(x => string.Equals(x.Priority, expected.Priority, StringComparison.Ordinal));
            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing decision-anchor priority distribution {snapshot.CanonicalCode}/{expected.Priority}.");
            }

            if (actual.Count != expected.Count || Math.Abs(actual.Share - expected.Share) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor priority distribution numeric mismatch for {snapshot.CanonicalCode}/{expected.Priority}. expectedCount={expected.Count}, actualCount={actual.Count}, expectedShare={expected.Share}, actualShare={actual.Share}");
            }

            if (!actual.AnchorKeys.SequenceEqual(expected.AnchorKeys, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor priority anchors mismatch for {snapshot.CanonicalCode}/{expected.Priority}. expected={string.Join(',', expected.AnchorKeys)}, actual={string.Join(',', actual.AnchorKeys)}");
            }

            if (!actual.SuggestedNextStepFocuses.SequenceEqual(expected.SuggestedNextStepFocuses, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor priority focuses mismatch for {snapshot.CanonicalCode}/{expected.Priority}. expected={string.Join(',', expected.SuggestedNextStepFocuses)}, actual={string.Join(',', actual.SuggestedNextStepFocuses)}");
            }

            if (!actual.SuggestedNextStepFields.SequenceEqual(expected.SuggestedNextStepFields, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor priority fields mismatch for {snapshot.CanonicalCode}/{expected.Priority}. expected={string.Join(',', expected.SuggestedNextStepFields)}, actual={string.Join(',', actual.SuggestedNextStepFields)}");
            }

            if (!actual.SuggestedNextSteps.SequenceEqual(expected.SuggestedNextSteps, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor priority next steps mismatch for {snapshot.CanonicalCode}/{expected.Priority}. expected={string.Join(" | ", expected.SuggestedNextSteps)}, actual={string.Join(" | ", actual.SuggestedNextSteps)}");
            }

            if (!string.Equals(actual.SuggestedNextStepSummary, expected.SuggestedNextStepSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor priority next-step summary mismatch for {snapshot.CanonicalCode}/{expected.Priority}. expected='{expected.SuggestedNextStepSummary}', actual='{actual.SuggestedNextStepSummary}'");
            }

            if (!string.Equals(actual.DominantAnchorKey, expected.DominantAnchorKey, StringComparison.Ordinal)
                || !string.Equals(actual.DominantSuggestedNextStepFocus, expected.DominantSuggestedNextStepFocus, StringComparison.Ordinal)
                || !actual.DominantSuggestedNextStepFields.SequenceEqual(expected.DominantSuggestedNextStepFields, StringComparer.Ordinal)
                || !string.Equals(actual.DominantSuggestedNextStepSummary, expected.DominantSuggestedNextStepSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor dominant priority payload mismatch for {snapshot.CanonicalCode}/{expected.Priority}. expectedTop={expected.DominantAnchorKey}:{expected.DominantSuggestedNextStepFocus}:{string.Join(',', expected.DominantSuggestedNextStepFields)}:'{expected.DominantSuggestedNextStepSummary}', actualTop={actual.DominantAnchorKey}:{actual.DominantSuggestedNextStepFocus}:{string.Join(',', actual.DominantSuggestedNextStepFields)}:'{actual.DominantSuggestedNextStepSummary}'");
            }
        }

        if (!string.Equals(snapshot.DecisionAnchorPrioritySummary, expectedSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor priority summary mismatch for {snapshot.CanonicalCode}. expected='{expectedSummary}', actual='{snapshot.DecisionAnchorPrioritySummary}'");
        }

        if (!snapshot.SuggestedDecisionAnchorNextSteps.SequenceEqual(expectedSuggestedNextSteps, StringComparer.Ordinal)
            || !string.Equals(snapshot.SuggestedDecisionAnchorNextStepSummary, expectedSuggestedNextSteps.Count == 0 ? "none" : string.Join("; ", expectedSuggestedNextSteps), StringComparison.Ordinal)
            || !string.Equals(snapshot.LegacyDecisionAnchorNextActionSummary, expectedNextActionSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.LegacyDecisionAnchorGapPreviewSummary, expectedGapPreviewSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.LegacyDecisionAnchorResolutionSummary, expectedResolutionSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor summary projection mismatch for {snapshot.CanonicalCode}. expectedSteps=[{string.Join(" | ", expectedSuggestedNextSteps)}], actualSteps=[{string.Join(" | ", snapshot.SuggestedDecisionAnchorNextSteps)}], expectedStepSummary='{(expectedSuggestedNextSteps.Count == 0 ? "none" : string.Join("; ", expectedSuggestedNextSteps))}', actualStepSummary='{snapshot.SuggestedDecisionAnchorNextStepSummary}', expectedNextAction='{expectedNextActionSummary}', actualNextAction='{snapshot.LegacyDecisionAnchorNextActionSummary}', expectedGapPreview='{expectedGapPreviewSummary}', actualGapPreview='{snapshot.LegacyDecisionAnchorGapPreviewSummary}', expectedResolutionSummary='{expectedResolutionSummary}', actualResolutionSummary='{snapshot.LegacyDecisionAnchorResolutionSummary}'");
        }

        foreach (var expected in expectedResolutions)
        {
            var actual = snapshot.LegacyDecisionAnchorResolutions.FirstOrDefault(x => string.Equals(x.AnchorKey, expected.AnchorKey, StringComparison.Ordinal));
            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing decision-anchor resolution {snapshot.CanonicalCode}/{expected.AnchorKey}.");
            }

            if (!string.Equals(actual.SuggestedPrimaryNextField, expected.SuggestedPrimaryNextField, StringComparison.Ordinal)
                || !string.Equals(actual.SuggestedPrimaryNextFieldSummary, expected.SuggestedPrimaryNextFieldSummary, StringComparison.Ordinal)
                || !string.Equals(actual.SuggestedNextStepPrioritySummary, expected.SuggestedNextStepPrioritySummary, StringComparison.Ordinal)
                || !string.Equals(actual.SuggestedNextStepCoverageSummary, expected.SuggestedNextStepCoverageSummary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor resolution detail mismatch for {snapshot.CanonicalCode}/{expected.AnchorKey}. expectedPrimary={expected.SuggestedPrimaryNextField}/'{expected.SuggestedPrimaryNextFieldSummary}', actualPrimary={actual.SuggestedPrimaryNextField}/'{actual.SuggestedPrimaryNextFieldSummary}', expectedPrioritySummary='{expected.SuggestedNextStepPrioritySummary}', actualPrioritySummary='{actual.SuggestedNextStepPrioritySummary}', expectedCoverageSummary='{expected.SuggestedNextStepCoverageSummary}', actualCoverageSummary='{actual.SuggestedNextStepCoverageSummary}'");
            }
        }

        var expectedTopPriority = expectedDistributions.FirstOrDefault();
        if (expectedTopPriority is null)
        {
            if (!string.IsNullOrWhiteSpace(snapshot.DecisionAnchorTopPriority)
                || !string.IsNullOrWhiteSpace(snapshot.DecisionAnchorTopPrioritySummary)
                || !string.IsNullOrWhiteSpace(snapshot.DecisionAnchorTopPriorityDominantAnchorKey)
                || !string.IsNullOrWhiteSpace(snapshot.DecisionAnchorTopPriorityFocus)
                || snapshot.DecisionAnchorTopPriorityFields.Count != 0
                || !string.IsNullOrWhiteSpace(snapshot.DecisionAnchorTopPriorityNextStepSummary)
                || !string.IsNullOrWhiteSpace(snapshot.DecisionAnchorTopPriorityPrimaryField)
                || !string.IsNullOrWhiteSpace(snapshot.DecisionAnchorTopPriorityPrimaryFieldSummary))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor top priority should be empty for {snapshot.CanonicalCode}.");
            }

            return;
        }

        var expectedTopPrioritySummary = $"top decision anchor priority={expectedTopPriority.Priority}; focus={expectedTopPriority.DominantSuggestedNextStepFocus}; anchor={expectedTopPriority.DominantAnchorKey}; fields={(expectedTopPriority.DominantSuggestedNextStepFields.Count == 0 ? "none" : string.Join(", ", expectedTopPriority.DominantSuggestedNextStepFields))}";
        var expectedTopResolution = expectedResolutions.FirstOrDefault(x => string.Equals(x.AnchorKey, expectedTopPriority.DominantAnchorKey, StringComparison.Ordinal));
        if (!string.Equals(snapshot.DecisionAnchorTopPriority, expectedTopPriority.Priority, StringComparison.Ordinal)
            || !string.Equals(snapshot.DecisionAnchorTopPriorityDominantAnchorKey, expectedTopPriority.DominantAnchorKey, StringComparison.Ordinal)
            || !string.Equals(snapshot.DecisionAnchorTopPriorityFocus, expectedTopPriority.DominantSuggestedNextStepFocus, StringComparison.Ordinal)
            || !snapshot.DecisionAnchorTopPriorityFields.SequenceEqual(expectedTopPriority.DominantSuggestedNextStepFields, StringComparer.Ordinal)
            || !string.Equals(snapshot.DecisionAnchorTopPriorityNextStepSummary, expectedTopPriority.DominantSuggestedNextStepSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.DecisionAnchorTopPrioritySummary, expectedTopPrioritySummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.DecisionAnchorTopPriorityPrimaryField, expectedTopResolution?.SuggestedPrimaryNextField ?? string.Empty, StringComparison.Ordinal)
            || !string.Equals(snapshot.DecisionAnchorTopPriorityPrimaryFieldSummary, expectedTopResolution?.SuggestedPrimaryNextFieldSummary ?? string.Empty, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: top decision-anchor priority mismatch for {snapshot.CanonicalCode}. expected={expectedTopPriority.Priority}/{expectedTopPriority.DominantAnchorKey}/{expectedTopPriority.DominantSuggestedNextStepFocus}/{string.Join(',', expectedTopPriority.DominantSuggestedNextStepFields)}/'{expectedTopPriority.DominantSuggestedNextStepSummary}'/'{expectedTopPrioritySummary}'/'{expectedTopResolution?.SuggestedPrimaryNextField ?? string.Empty}'/'{expectedTopResolution?.SuggestedPrimaryNextFieldSummary ?? string.Empty}', actual={snapshot.DecisionAnchorTopPriority}/{snapshot.DecisionAnchorTopPriorityDominantAnchorKey}/{snapshot.DecisionAnchorTopPriorityFocus}/{string.Join(',', snapshot.DecisionAnchorTopPriorityFields)}/'{snapshot.DecisionAnchorTopPriorityNextStepSummary}'/'{snapshot.DecisionAnchorTopPrioritySummary}'/'{snapshot.DecisionAnchorTopPriorityPrimaryField}'/'{snapshot.DecisionAnchorTopPriorityPrimaryFieldSummary}'");
        }
    }

    private static void AssertDecisionAnchorPrimaryFieldDistribution(MotorYMethodAdaptationPlanSnapshot snapshot)
    {
        var expectedResolutions = snapshot.LegacyDecisionAnchorResolutions
            .Select(x => new MotorYDecisionAnchorResolution
            {
                AnchorKey = x.AnchorKey,
                ResolvedByObservedPayload = x.ResolvedByObservedPayload,
                PartiallyResolvedByObservedPayload = x.PartiallyResolvedByObservedPayload,
                RequiredPayloadFields = x.RequiredPayloadFields,
                ObservedPayloadFields = x.ObservedPayloadFields,
                MissingPayloadFields = x.MissingPayloadFields,
                CoverageRatio = x.CoverageRatio,
                CoveragePercentagePoints = x.CoveragePercentagePoints,
                ResolutionStage = x.ResolutionStage,
                SuggestedNextStepCategory = x.SuggestedNextStepCategory,
                SuggestedNextStepFocus = x.SuggestedNextStepFocus,
                SuggestedNextStepFields = x.SuggestedNextStepFields,
                SuggestedNextSteps = x.SuggestedNextSteps,
                SuggestedNextStepSummary = x.SuggestedNextStepSummary,
                SuggestedNextStepPriority = x.SuggestedNextStepPriority,
                SuggestedNextStepPrioritySummary = x.SuggestedNextStepPrioritySummary,
                SuggestedNextStepCoverageSummary = x.SuggestedNextStepCoverageSummary,
                SuggestedPrimaryNextField = x.SuggestedPrimaryNextField,
                SuggestedPrimaryNextFieldSummary = x.SuggestedPrimaryNextFieldSummary,
                Summary = x.Summary
            })
            .ToArray();
        var expectedDistributions = MotorYDecisionAnchorResolutionFactory.BuildPrimaryFieldDistributions(expectedResolutions);
        var expectedSummary = MotorYDecisionAnchorResolutionFactory.BuildPrimaryFieldSummary(expectedResolutions);

        if (snapshot.DecisionAnchorPrimaryFieldDistributions.Count != expectedDistributions.Count)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor primary-field distribution count mismatch for {snapshot.CanonicalCode}. expected={expectedDistributions.Count}, actual={snapshot.DecisionAnchorPrimaryFieldDistributions.Count}");
        }

        foreach (var expected in expectedDistributions)
        {
            var actual = snapshot.DecisionAnchorPrimaryFieldDistributions.FirstOrDefault(x => string.Equals(x.PrimaryField, expected.PrimaryField, StringComparison.Ordinal));
            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing decision-anchor primary-field distribution {snapshot.CanonicalCode}/{expected.PrimaryField}.");
            }

            if (actual.Count != expected.Count || Math.Abs(actual.Share - expected.Share) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor primary-field numeric mismatch for {snapshot.CanonicalCode}/{expected.PrimaryField}. expectedCount={expected.Count}, actualCount={actual.Count}, expectedShare={expected.Share}, actualShare={actual.Share}");
            }

            if (!actual.AnchorKeys.SequenceEqual(expected.AnchorKeys, StringComparer.Ordinal)
                || !actual.SuggestedNextStepFocuses.SequenceEqual(expected.SuggestedNextStepFocuses, StringComparer.Ordinal)
                || !actual.SuggestedNextStepPriorities.SequenceEqual(expected.SuggestedNextStepPriorities, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor primary-field payload mismatch for {snapshot.CanonicalCode}/{expected.PrimaryField}. expectedAnchors={string.Join(',', expected.AnchorKeys)}, actualAnchors={string.Join(',', actual.AnchorKeys)}, expectedFocuses={string.Join(',', expected.SuggestedNextStepFocuses)}, actualFocuses={string.Join(',', actual.SuggestedNextStepFocuses)}, expectedPriorities={string.Join(',', expected.SuggestedNextStepPriorities)}, actualPriorities={string.Join(',', actual.SuggestedNextStepPriorities)}");
            }

            if (actual.CanonicalCodes.Count != 1 || !string.Equals(actual.CanonicalCodes[0], snapshot.CanonicalCode, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor primary-field canonical-code projection mismatch for {snapshot.CanonicalCode}/{expected.PrimaryField}. actualCodes={string.Join(',', actual.CanonicalCodes)}");
            }

            if (!string.Equals(actual.Summary, expected.Summary, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor primary-field summary mismatch for {snapshot.CanonicalCode}/{expected.PrimaryField}. expected='{expected.Summary}', actual='{actual.Summary}'");
            }
        }

        if (!string.Equals(snapshot.DecisionAnchorPrimaryFieldSummary, expectedSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: decision-anchor primary-field summary mismatch for {snapshot.CanonicalCode}. expected='{expectedSummary}', actual='{snapshot.DecisionAnchorPrimaryFieldSummary}'");
        }
    }

    private static void AssertLegacyCodeSelection(
        MotorYMethodAdaptationPlanSnapshot snapshot,
        SqliteConnection connection,
        string canonicalCode)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COALESCE(Code, ''), COUNT(*)
FROM TestRecordItems
WHERE Code IN (
    '直流电阻测定',
    '空载试验',
    '空载特性试验',
    '热试验',
    'A法负载试验',
    'B法负载试验',
    '堵转试验',
    '堵转特性试验')
GROUP BY COALESCE(Code, '');";

        var rows = new List<(string LegacyCode, int Count)>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var legacyCode = reader.GetString(0);
            if (!string.Equals(MotorYLegacyItemCodeNormalizer.Normalize(legacyCode), canonicalCode, StringComparison.Ordinal))
            {
                continue;
            }

            rows.Add((legacyCode, reader.GetInt32(1)));
        }

        var total = rows.Sum(x => x.Count);
        var ordered = rows
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.LegacyCode, StringComparer.Ordinal)
            .ToArray();
        var expected = ordered.FirstOrDefault();
        var expectedShare = total <= 0
            ? 0d
            : Math.Round((double)expected.Count / total, 4, MidpointRounding.AwayFromZero);
        var expectedSummary = expected == default
            ? $"legacy code selection unavailable for {canonicalCode}"
            : $"recommended legacy code '{expected.LegacyCode}' for {canonicalCode} ({expected.Count}/{total}, {(int)Math.Round(expectedShare * 100d, MidpointRounding.AwayFromZero)}pp)";

        if (!string.Equals(snapshot.RecommendedLegacyCode, expected.LegacyCode, StringComparison.Ordinal)
            || !string.Equals(snapshot.DominantLegacyCode, expected.LegacyCode, StringComparison.Ordinal)
            || snapshot.RecommendedLegacyCodeCount != expected.Count
            || Math.Abs(snapshot.RecommendedLegacyCodeShare - expectedShare) > 0.0001d
            || !string.Equals(snapshot.LegacyCodeSelectionSummary, expectedSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code selection mismatch for {canonicalCode}. expectedCode={expected.LegacyCode}, actualCode={snapshot.RecommendedLegacyCode}, expectedCount={expected.Count}, actualCount={snapshot.RecommendedLegacyCodeCount}, expectedShare={expectedShare}, actualShare={snapshot.RecommendedLegacyCodeShare}, expectedSummary='{expectedSummary}', actualSummary='{snapshot.LegacyCodeSelectionSummary}'");
        }

        if (snapshot.LegacyCodeDistributions.Count != ordered.Length)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution count mismatch for {canonicalCode}. expected={ordered.Length}, actual={snapshot.LegacyCodeDistributions.Count}");
        }

        foreach (var row in ordered)
        {
            var actual = snapshot.LegacyCodeDistributions.FirstOrDefault(x => string.Equals(x.LegacyCode, row.LegacyCode, StringComparison.Ordinal));
            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: missing legacy-code distribution {canonicalCode}/{row.LegacyCode}.");
            }

            var share = total <= 0 ? 0d : Math.Round((double)row.Count / total, 4, MidpointRounding.AwayFromZero);
            if (actual.Count != row.Count || Math.Abs(actual.Share - share) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution mismatch for {canonicalCode}/{row.LegacyCode}. expectedCount={row.Count}, actualCount={actual.Count}, expectedShare={share}, actualShare={actual.Share}");
            }
        }
    }

    private static void AssertLegacyCodeDistributionConsistency(
        MotorYMethodAdaptationPlanSnapshot snapshot,
        IReadOnlyList<StpDbMotorYLegacyCodeDistributionSnapshot> legacyCodeDistributionRows,
        string canonicalCode)
    {
        var expectedRows = legacyCodeDistributionRows
            .Where(x => string.Equals(x.CanonicalCode, canonicalCode, StringComparison.Ordinal))
            .GroupBy(x => x.LegacyCode, StringComparer.Ordinal)
            .Select(group => new
            {
                LegacyCode = group.Key,
                Count = group.Sum(x => x.Count)
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.LegacyCode, StringComparer.Ordinal)
            .ToArray();

        if (expectedRows.Length != snapshot.LegacyCodeDistributions.Count)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution service cross-check count mismatch for {canonicalCode}. expected={expectedRows.Length}, actual={snapshot.LegacyCodeDistributions.Count}");
        }

        var total = expectedRows.Sum(x => x.Count);
        foreach (var row in expectedRows)
        {
            var expectedShare = total <= 0
                ? 0d
                : Math.Round((double)row.Count / total, 4, MidpointRounding.AwayFromZero);
            var actual = snapshot.LegacyCodeDistributions.FirstOrDefault(x => string.Equals(x.LegacyCode, row.LegacyCode, StringComparison.Ordinal));
            if (actual is null)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution service cross-check missing row {canonicalCode}/{row.LegacyCode}.");
            }

            if (actual.Count != row.Count || Math.Abs(actual.Share - expectedShare) > 0.0001d)
            {
                throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution service cross-check mismatch for {canonicalCode}/{row.LegacyCode}. expectedCount={row.Count}, actualCount={actual.Count}, expectedShare={expectedShare}, actualShare={actual.Share}");
            }
        }

        var expectedRecommended = expectedRows.FirstOrDefault();
        if (expectedRecommended is null)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: no expected recommended legacy code for {canonicalCode}.");
        }

        if (!string.Equals(snapshot.RecommendedLegacyCode, expectedRecommended.LegacyCode, StringComparison.Ordinal)
            || !string.Equals(snapshot.DominantLegacyCode, expectedRecommended.LegacyCode, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: legacy-code distribution service cross-check recommended/dominant mismatch for {canonicalCode}. expected='{expectedRecommended.LegacyCode}', recommended='{snapshot.RecommendedLegacyCode}', dominant='{snapshot.DominantLegacyCode}'");
        }
    }

    private static void AssertSampleCountReadiness(MotorYMethodAdaptationPlanSnapshot snapshot)
    {
        var expectedRawReady = snapshot.RawDataSampleCount >= snapshot.MinimumRawSampleCount;
        var expectedStructuredPayloadReady = snapshot.StructuredPayloadSampleCount >= snapshot.MinimumStructuredPayloadSampleCount;
        var expectedStructuredResultReady = snapshot.StructuredResultSampleCount >= snapshot.MinimumStructuredResultSampleCount;

        var expectedRawSummary = snapshot.MinimumRawSampleCount <= 0
            ? $"raw sample count requirement not set; observed {snapshot.RawDataSampleCount}"
            : expectedRawReady
                ? $"raw sample count ready {snapshot.RawDataSampleCount}/{snapshot.MinimumRawSampleCount}"
                : $"raw sample count insufficient {snapshot.RawDataSampleCount}/{snapshot.MinimumRawSampleCount}";
        var expectedStructuredPayloadSummary = snapshot.MinimumStructuredPayloadSampleCount <= 0
            ? $"structured payload sample count requirement not set; observed {snapshot.StructuredPayloadSampleCount}"
            : expectedStructuredPayloadReady
                ? $"structured payload sample count ready {snapshot.StructuredPayloadSampleCount}/{snapshot.MinimumStructuredPayloadSampleCount}"
                : $"structured payload sample count insufficient {snapshot.StructuredPayloadSampleCount}/{snapshot.MinimumStructuredPayloadSampleCount}";
        var expectedStructuredResultSummary = snapshot.MinimumStructuredResultSampleCount <= 0
            ? $"structured result sample count requirement not set; observed {snapshot.StructuredResultSampleCount}"
            : expectedStructuredResultReady
                ? $"structured result sample count ready {snapshot.StructuredResultSampleCount}/{snapshot.MinimumStructuredResultSampleCount}"
                : $"structured result sample count insufficient {snapshot.StructuredResultSampleCount}/{snapshot.MinimumStructuredResultSampleCount}";

        var expectedRawGap = Math.Max(0, snapshot.MinimumRawSampleCount - snapshot.RawDataSampleCount);
        var expectedStructuredPayloadGap = Math.Max(0, snapshot.MinimumStructuredPayloadSampleCount - snapshot.StructuredPayloadSampleCount);
        var expectedStructuredResultGap = Math.Max(0, snapshot.MinimumStructuredResultSampleCount - snapshot.StructuredResultSampleCount);
        var expectedRawDecisionSummary = snapshot.MinimumRawSampleCount <= 0
            ? $"raw sample count gate disabled for {snapshot.CanonicalCode}; observed {snapshot.RawDataSampleCount}"
            : expectedRawReady
                ? $"raw sample count gate passed for {snapshot.CanonicalCode}: observed {snapshot.RawDataSampleCount} >= required {snapshot.MinimumRawSampleCount}"
                : $"raw sample count gate blocked for {snapshot.CanonicalCode}: observed {snapshot.RawDataSampleCount}, still need {expectedRawGap} more samples to reach {snapshot.MinimumRawSampleCount}";
        var expectedStructuredPayloadDecisionSummary = snapshot.MinimumStructuredPayloadSampleCount <= 0
            ? $"structured payload sample count gate disabled for {snapshot.CanonicalCode}; observed {snapshot.StructuredPayloadSampleCount}"
            : expectedStructuredPayloadReady
                ? $"structured payload sample count gate passed for {snapshot.CanonicalCode}: observed {snapshot.StructuredPayloadSampleCount} >= required {snapshot.MinimumStructuredPayloadSampleCount}"
                : $"structured payload sample count gate blocked for {snapshot.CanonicalCode}: observed {snapshot.StructuredPayloadSampleCount}, still need {expectedStructuredPayloadGap} more samples to reach {snapshot.MinimumStructuredPayloadSampleCount}";
        var expectedStructuredResultDecisionSummary = snapshot.MinimumStructuredResultSampleCount <= 0
            ? $"structured result sample count gate disabled for {snapshot.CanonicalCode}; observed {snapshot.StructuredResultSampleCount}"
            : expectedStructuredResultReady
                ? $"structured result sample count gate passed for {snapshot.CanonicalCode}: observed {snapshot.StructuredResultSampleCount} >= required {snapshot.MinimumStructuredResultSampleCount}"
                : $"structured result sample count gate blocked for {snapshot.CanonicalCode}: observed {snapshot.StructuredResultSampleCount}, still need {expectedStructuredResultGap} more samples to reach {snapshot.MinimumStructuredResultSampleCount}";

        if (snapshot.RawSampleCountReady != expectedRawReady
            || snapshot.StructuredPayloadSampleCountReady != expectedStructuredPayloadReady
            || snapshot.StructuredResultSampleCountReady != expectedStructuredResultReady
            || snapshot.RawSampleCountGap != expectedRawGap
            || snapshot.StructuredPayloadSampleCountGap != expectedStructuredPayloadGap
            || snapshot.StructuredResultSampleCountGap != expectedStructuredResultGap
            || !string.Equals(snapshot.RawSampleCountReadinessSummary, expectedRawSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.StructuredPayloadSampleCountReadinessSummary, expectedStructuredPayloadSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.StructuredResultSampleCountReadinessSummary, expectedStructuredResultSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.RawSampleCountDecisionSummary, expectedRawDecisionSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.StructuredPayloadSampleCountDecisionSummary, expectedStructuredPayloadDecisionSummary, StringComparison.Ordinal)
            || !string.Equals(snapshot.StructuredResultSampleCountDecisionSummary, expectedStructuredResultDecisionSummary, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: sample-count readiness mismatch for {snapshot.CanonicalCode}. raw='{snapshot.RawSampleCountReadinessSummary}' gap={snapshot.RawSampleCountGap} decision='{snapshot.RawSampleCountDecisionSummary}', structuredPayload='{snapshot.StructuredPayloadSampleCountReadinessSummary}' gap={snapshot.StructuredPayloadSampleCountGap} decision='{snapshot.StructuredPayloadSampleCountDecisionSummary}', structuredResult='{snapshot.StructuredResultSampleCountReadinessSummary}' gap={snapshot.StructuredResultSampleCountGap} decision='{snapshot.StructuredResultSampleCountDecisionSummary}'");
        }
    }

    private static void AssertRoute(MotorYLegacyAlgorithmRoute? route, string canonicalCode, int methodValue, string context)
    {
        var expected = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, methodValue)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: expected route missing for {context}.");

        if (route is null)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: route missing for {context}.");
        }

        if (!string.Equals(route.CanonicalCode, expected.CanonicalCode, StringComparison.Ordinal)
            || route.MethodValue != expected.MethodValue
            || !string.Equals(route.MethodKey, expected.MethodKey, StringComparison.Ordinal)
            || !string.Equals(route.ProfileKey, expected.ProfileKey, StringComparison.Ordinal)
            || !string.Equals(route.VariantKind, expected.VariantKind, StringComparison.Ordinal)
            || !string.Equals(route.AlgorithmFamily, expected.AlgorithmFamily, StringComparison.Ordinal)
            || !string.Equals(route.LegacyEnumName, expected.LegacyEnumName, StringComparison.Ordinal)
            || !string.Equals(route.LegacyFormName, expected.LegacyFormName, StringComparison.Ordinal)
            || !string.Equals(route.LegacyAlgorithmEntry, expected.LegacyAlgorithmEntry, StringComparison.Ordinal)
            || !string.Equals(route.LegacyMethodName, expected.LegacyMethodName, StringComparison.Ordinal)
            || !string.Equals(route.LegacySettingsMethodName, expected.LegacySettingsMethodName, StringComparison.Ordinal)
            || route.IsBaselineMethod != expected.IsBaselineMethod)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: route projection mismatch for {context}.");
        }
    }

    private static (string CanonicalCode, int TotalCount, int BaselineMethod, int BaselineCount, double BaselineShare, int DominantMethod, int DominantCount, double DominantShare, int SelectedMethod, int SelectedCount, double SelectedShare, string SelectionStrategy, bool ShouldUseDominantRoute, int DominantLeadCount, int DominantLeadPercentagePoints, double SelectedLeadCountVsBaseline, int SelectedLeadPercentagePointsVsBaseline, string SelectionReason, string SelectedMethodSummary, string BaselineDominantComparisonSummary, IReadOnlyList<(int MethodValue, int Count, double Share)> Distributions) BuildExpected(
        SqliteConnection connection,
        string canonicalCode,
        int baselineMethod)
    {
        var rows = LoadCounts(connection, canonicalCode);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: no source rows for {canonicalCode}.");
        }

        var baselineCount = rows.TryGetValue(baselineMethod, out var baseline) ? baseline : 0;
        var dominant = rows
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key)
            .First();
        var totalCount = rows.Values.Sum();
        var dominantShare = totalCount <= 0
            ? 0d
            : Math.Round((double)dominant.Value / totalCount, 4, MidpointRounding.AwayFromZero);
        var shouldUseDominantRoute = baselineMethod != dominant.Key
            && dominantShare >= DominantOverrideThreshold;
        var selectedMethod = shouldUseDominantRoute ? dominant.Key : baselineMethod;
        var selectedCount = shouldUseDominantRoute ? dominant.Value : baselineCount;
        var selectionStrategy = shouldUseDominantRoute
            ? "dominant-threshold-over-baseline"
            : "baseline";
        var baselineShare = totalCount <= 0
            ? 0d
            : Math.Round((double)baselineCount / totalCount, 4, MidpointRounding.AwayFromZero);
        var selectedShare = totalCount <= 0
            ? 0d
            : Math.Round((double)selectedCount / totalCount, 4, MidpointRounding.AwayFromZero);
        var dominantLeadCount = Math.Max(0, dominant.Value - baselineCount);
        var dominantLeadPercentagePoints = Math.Max(0, (int)Math.Round((dominantShare - baselineShare) * 100d, MidpointRounding.AwayFromZero));
        var selectedLeadCountVsBaseline = Math.Max(0d, selectedCount - baselineCount);
        var selectedLeadPercentagePointsVsBaseline = Math.Max(0, (int)Math.Round((selectedShare - baselineShare) * 100d, MidpointRounding.AwayFromZero));
        var selectionReason = shouldUseDominantRoute
            ? $"selected dominant method {dominant.Key} over baseline {baselineMethod} because dominant share {dominantShare:P2} reached threshold {DominantOverrideThreshold:P0} (+{dominantLeadCount} items, +{dominantLeadPercentagePoints}pp)"
            : baselineMethod == dominant.Key
                ? $"kept baseline method {selectedMethod} because baseline already matches dominant distribution ({dominantShare:P2})"
                : $"kept baseline method {baselineMethod} because dominant method {dominant.Key} share {dominantShare:P2} did not reach threshold {DominantOverrideThreshold:P0}";
        var selectedRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, selectedMethod)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: selected route missing for {canonicalCode}:{selectedMethod}.");
        var selectedMethodSummary = $"selected {selectedRoute.LegacyMethodName ?? canonicalCode} method {selectedMethod} ({selectedRoute.VariantKind}) covering {selectedCount}/{totalCount} items ({selectedShare:P2})";
        var baselineRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, baselineMethod)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: baseline route missing for {canonicalCode}:{baselineMethod}.");
        var dominantRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(canonicalCode, dominant.Key)
            ?? throw new InvalidOperationException($"stp.db Motor_Y method adaptation plan smoke test failed: dominant route missing for {canonicalCode}:{dominant.Key}.");
        var baselineDominantComparisonSummary = $"baseline {baselineMethod} ({baselineRoute.VariantKind})={baselineCount}/{totalCount} ({baselineShare:P2}), dominant {dominant.Key} ({dominantRoute.VariantKind})={dominant.Value}/{totalCount} ({dominantShare:P2})";
        var distributions = rows
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key)
            .Select(x => (
                MethodValue: x.Key,
                Count: x.Value,
                Share: totalCount <= 0
                    ? 0d
                    : Math.Round((double)x.Value / totalCount, 4, MidpointRounding.AwayFromZero)))
            .ToArray();

        return (canonicalCode, totalCount, baselineMethod, baselineCount, baselineShare, dominant.Key, dominant.Value, dominantShare, selectedMethod, selectedCount, selectedShare, selectionStrategy, shouldUseDominantRoute, dominantLeadCount, dominantLeadPercentagePoints, selectedLeadCountVsBaseline, selectedLeadPercentagePointsVsBaseline, selectionReason, selectedMethodSummary, baselineDominantComparisonSummary, distributions);
    }

    private static Dictionary<int, int> LoadCounts(SqliteConnection connection, string canonicalCode)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Code, Method, COUNT(*)
FROM TestRecordItems
WHERE Code IN (
    '直流电阻测定',
    '空载试验',
    '空载特性试验',
    '热试验',
    'A法负载试验',
    'B法负载试验',
    '堵转试验',
    '堵转特性试验')
  AND Method IS NOT NULL
GROUP BY Code, Method;";

        var result = new Dictionary<int, int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var normalized = MotorYLegacyItemCodeNormalizer.Normalize(reader.GetString(0));
            if (!string.Equals(normalized, canonicalCode, StringComparison.Ordinal))
            {
                continue;
            }

            var method = reader.GetInt32(1);
            var count = reader.GetInt32(2);
            result[method] = result.TryGetValue(method, out var current)
                ? current + count
                : count;
        }

        return result;
    }
}
