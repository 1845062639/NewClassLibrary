using StandardTestNext.Contracts;

namespace StandardTestNext.Test.Application.Services;

public static class MotorYLegacyAlgorithmDependencyCatalogSmokeTests
{
    public static void Run()
    {
        var expected = new[]
        {
            (MotorYTestMethodCodes.DcResistance, false, Array.Empty<string>(), new[] { "Ruv", "Rvw", "Rwu", "R1", "R1c", "θ1c" }, Array.Empty<string>(), new[] { "R1", "R1c", "θ1c" }, new[] { "R1", "R1c", "θ1c" }),
            (MotorYTestMethodCodes.NoLoad, false, new[] { MotorYTestMethodCodes.DcResistance }, new[] { "DataList", "Un", "R1c", "θ1c", "K1", "Order" }, Array.Empty<string>(), new[] { "I0", "ΔI0", "P0", "Pcu", "Pfw", "Pfe", "CoefficientOfPfe" }, new[] { "R0", "θ0", "Pcon", "P0cu1", "Pfw", "Pfe", "CoefficientOfPfe" }),
            (MotorYTestMethodCodes.HeatRun, true, new[] { MotorYTestMethodCodes.DcResistance }, new[] { "Data1List", "Data2List", "Rc", "θc", "Pn", "K1", "Order", "HotStateType" }, new[] { "GB" }, new[] { "Rw", "Rn", "Δθ", "Δθn", "θw", "θs", "θb" }, new[] { "firstSecondsInterval", "Rw", "Rn", "Rws", "θw", "θs", "θb" }),
            (MotorYTestMethodCodes.LoadA, false, new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θa", "PolePairs", "Pn", "Un", "ΔT" }, Array.Empty<string>(), new[] { "Pcu1", "Pcu2", "ResultDataList", "η" }, new[] { "R1t", "Pcu1t", "Nst", "St", "Ub", "Pfe", "Pcu2t", "Tx", "P2tx", "P2x", "η" }),
            (MotorYTestMethodCodes.LoadB, true, new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun }, new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θw", "θb", "PolePairs", "Pn", "Un", "ΔT", "K1", "K2" }, new[] { "GB" }, new[] { "A", "B", "R", "Pcu1", "Pcu2", "θs", "ResultDataList" }, new[] { "R1t", "Pcu1t", "Nst", "St", "Ub", "Pfe", "Pcu2t", "Tx", "P2tx", "Pl", "A", "B", "R", "Ps", "θs" }),
            (MotorYTestMethodCodes.LockedRotor, false, new[] { MotorYTestMethodCodes.NoLoad }, new[] { "DataList", "CoefficientOfPfe", "Un", "In", "Tn", "PolePairs", "R1c", "θ1c", "K1", "C1" }, Array.Empty<string>(), new[] { "Ikn", "Pkn", "Tkn", "IknDivideIn", "TknDivideTn" }, new[] { "θ1s", "R", "Pkcu1", "Pfe", "ns", "Tk", "Ikn", "Pkn", "Tkn" })
        };

        foreach (var row in expected)
        {
            var profile = MotorYLegacyAlgorithmDependencyCatalog.TryGet(row.Item1)
                ?? throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: missing profile for {row.Item1}.");

            if (profile.RequiresRatedParams != row.Item2)
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: RequiresRatedParams mismatch for {row.Item1}.");
            }

            if (!profile.UpstreamCanonicalCodes.SequenceEqual(row.Item3, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: upstream mismatch for {row.Item1}.");
            }

            if (!profile.RequiredPayloadFields.SequenceEqual(row.Item4, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: payload fields mismatch for {row.Item1}.");
            }

            if (!profile.RequiredRatedParamFields.SequenceEqual(row.Item5, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: rated param fields mismatch for {row.Item1}.");
            }

            if (!profile.RequiredResultFields.SequenceEqual(row.Item6, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: result fields mismatch for {row.Item1}.");
            }

            if (!profile.RequiredIntermediateResultFields.SequenceEqual(row.Item7, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: intermediate result fields mismatch for {row.Item1}.");
            }

            if (profile.RequiredStructuredPayloadSignals.Count == 0
                || profile.RequiredStructuredResultSignals.Count == 0)
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: structured signal dependency missing for {row.Item1}.");
            }

            if (string.IsNullOrWhiteSpace(profile.Notes)
                || string.IsNullOrWhiteSpace(profile.AlgorithmEntry)
                || profile.FormulaSignals.Count == 0
                || profile.LegacyAlgorithmRules.Count == 0
                || profile.LegacyDecisionAnchors.Count == 0)
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: notes/algorithm entry/formula rules/decision anchors missing for {row.Item1}.");
            }

            if (profile.LegacyDecisionAnchorRequiredFields.Count == 0)
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: decision anchor required fields missing for {row.Item1}.");
            }

            if (row.Item1 != MotorYTestMethodCodes.DcResistance && profile.FormDependencyEvidences.Count == 0)
            {
                throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: form dependency evidence missing for {row.Item1}.");
            }
        }

        var decision = new MotorYMethodDecisionSnapshot
        {
            CanonicalCode = MotorYTestMethodCodes.LoadB,
            TotalCount = 10,
            BaselineRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 5),
            BaselineCount = 4,
            DominantRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 51),
            DominantCount = 6,
            RecommendedRoute = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 5),
            RecommendedStrategy = "baseline",
            ShouldPrioritizeDominantOverBaseline = false,
            DominantShare = 0.6,
            BaselineShare = 0.4,
            DominantOverrideThreshold = 0.7,
            DominantLeadCount = 2,
            DominantLeadPercentagePoints = 20,
            RecommendationReason = "test",
            RecommendedMethodSummary = "test",
            BaselineDominantComparisonSummary = "test",
            Distributions =
            [
                new MotorYMethodDistributionSnapshot
                {
                    MethodValue = 5,
                    Count = 4,
                    Share = 0.4,
                    Route = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 5)
                },
                new MotorYMethodDistributionSnapshot
                {
                    MethodValue = 51,
                    Count = 6,
                    Share = 0.6,
                    Route = MotorYLegacyAlgorithmRouteResolver.Resolve(MotorYTestMethodCodes.LoadB, 51)
                }
            ]
        };

        var dcResistanceDecisionAnchorObservationRules = MotorYObservedAlgorithmEvidenceCatalog.BuildDecisionAnchorObservationRules(
            MotorYTestMethodCodes.DcResistance,
            new[] { "R1c", "θ1c" },
            null);

        if (dcResistanceDecisionAnchorObservationRules.Count != 2
            || !dcResistanceDecisionAnchorObservationRules.All(rule => rule.CoveredByObservedPayload)
            || !dcResistanceDecisionAnchorObservationRules.All(rule => rule.ObservedPayloadFields.SequenceEqual(new[] { "R1", "R1c", "θ1c" }, StringComparer.Ordinal))
            || !dcResistanceDecisionAnchorObservationRules.All(rule => rule.MissingPayloadFields.Count == 0)
            || !dcResistanceDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "cold-baseline-ready", StringComparison.Ordinal)
                && string.Equals(rule.Summary, "decision-anchor-observation:cold-baseline-ready covered by observed payload fields 'R1', 'R1c', 'θ1c'", StringComparison.Ordinal))
            || !dcResistanceDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "downstream-ready", StringComparison.Ordinal)
                && string.Equals(rule.Summary, "decision-anchor-observation:downstream-ready covered by observed payload fields 'R1', 'R1c', 'θ1c'", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: decision anchor observation alias projection mismatch for DcResistance. actual=[{string.Join(" | ", dcResistanceDecisionAnchorObservationRules.Select(rule => $"{rule.AnchorKey}:{rule.CoveredByObservedPayload}:{string.Join(",", rule.ObservedPayloadFields)}:{string.Join(",", rule.MissingPayloadFields)}"))}]");
        }

        var heatRunDecisionAnchorObservationRules = MotorYObservedAlgorithmEvidenceCatalog.BuildDecisionAnchorObservationRules(
            MotorYTestMethodCodes.HeatRun,
            new[] { "HotStateType", "GB", "Pn", "Rw", "θb", "θw" },
            null);

        if (heatRunDecisionAnchorObservationRules.Count != 3
            || !heatRunDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "first-seconds-interval", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "Pn" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0)
            || !heatRunDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "hot-state-branch", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "HotStateType" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0)
            || !heatRunDecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "gb-temperature-branch", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "GB", "Rn", "θb", "θs", "θw" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0
                && string.Equals(rule.Summary, "decision-anchor-observation:gb-temperature-branch covered by observed payload fields 'GB', 'Rn', 'θb', 'θs', 'θw'", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: decision anchor observation alias projection mismatch for HeatRun. actual=[{string.Join(" | ", heatRunDecisionAnchorObservationRules.Select(rule => $"{rule.AnchorKey}:{rule.CoveredByObservedPayload}:{string.Join(",", rule.ObservedPayloadFields)}:{string.Join(",", rule.MissingPayloadFields)}"))}]");
        }

        var loadADecisionAnchorObservationRules = MotorYObservedAlgorithmEvidenceCatalog.BuildDecisionAnchorObservationRules(
            MotorYTestMethodCodes.LoadA,
            new[] { "CoefficientOfPfe", "Pfw", "ResultDataList", "θa" },
            null);

        if (loadADecisionAnchorObservationRules.Count != 3
            || !loadADecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "upstream-ready", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "CoefficientOfPfe", "Pfw", "θa" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0)
            || !loadADecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "rated-load-fit-grid", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "ResultDataList" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0)
            || !loadADecisionAnchorObservationRules.Any(rule => string.Equals(rule.AnchorKey, "payload-rated-quantity-ready", StringComparison.Ordinal)
                && rule.CoveredByObservedPayload
                && rule.ObservedPayloadFields.SequenceEqual(new[] { "Pcu1", "Pcu2", "η" }, StringComparer.Ordinal)
                && rule.MissingPayloadFields.Count == 0
                && string.Equals(rule.Summary, "decision-anchor-observation:payload-rated-quantity-ready covered by observed payload fields 'Pcu1', 'Pcu2', 'η'", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: decision anchor observation alias projection mismatch for LoadA. actual=[{string.Join(" | ", loadADecisionAnchorObservationRules.Select(rule => $"{rule.AnchorKey}:{rule.CoveredByObservedPayload}:{string.Join(",", rule.ObservedPayloadFields)}:{string.Join(",", rule.MissingPayloadFields)}"))}]");
        }

        var loadBDecisionAnchorObservationGaps = MotorYObservedAlgorithmEvidenceCatalog.BuildDecisionAnchorObservationGaps(
            MotorYTestMethodCodes.LoadB,
            new[] { "A", "B", "GB", "R", "ResultDataList", "θb", "θs", "θw" },
            null);

        if (loadBDecisionAnchorObservationGaps.Count != 4
            || loadBDecisionAnchorObservationGaps.Count(gap => gap.CoveredByObservedPayload) != 3
            || loadBDecisionAnchorObservationGaps.Count(gap => !gap.CoveredByObservedPayload) != 1
            || !loadBDecisionAnchorObservationGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor-observation:gb-ratios-branch", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "GB", "ratios", "θs" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0)
            || !loadBDecisionAnchorObservationGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor-observation:correlation-refit", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "A", "B", "R", "bad-point-refit" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0)
            || !loadBDecisionAnchorObservationGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor-observation:thermal-carryover", StringComparison.Ordinal)
                && gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "θb", "θw" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.Count == 0)
            || !loadBDecisionAnchorObservationGaps.Any(gap => string.Equals(gap.SignalOrRule, "decision-anchor-observation:ps-iteration", StringComparison.Ordinal)
                && !gap.CoveredByObservedPayload
                && gap.ObservedPayloadFields.SequenceEqual(new[] { "ResultDataList", "cuC" }, StringComparer.Ordinal)
                && gap.MissingPayloadFields.SequenceEqual(new[] { "Ps" }, StringComparer.Ordinal)
                && string.Equals(gap.Summary, "decision-anchor-observation:ps-iteration missing observed payload fields 'Ps'", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: decision anchor observation rule projection mismatch for LoadB. actual=[{string.Join(" | ", loadBDecisionAnchorObservationGaps.Select(gap => $"{gap.SignalOrRule}:{gap.CoveredByObservedPayload}:{string.Join(",", gap.ObservedPayloadFields)}:{string.Join(",", gap.MissingPayloadFields)}"))}]");
        }

        var contract = MotorYMethodAdaptationPlanContractMapper.Map(decision, route => route is null
            ? null
            : new MotorYBuildProfileContract
            {
                CanonicalCode = route.CanonicalCode,
                MethodValue = route.MethodValue,
                MethodKey = route.MethodKey,
                ProfileKey = route.ProfileKey,
                VariantKind = route.VariantKind,
                AlgorithmFamily = route.AlgorithmFamily,
                LegacyEnumName = route.LegacyEnumName,
                LegacyFormName = route.LegacyFormName,
                LegacyAlgorithmEntry = route.LegacyAlgorithmEntry,
                LegacyMethodName = route.LegacyMethodName,
                LegacySettingsMethodName = route.LegacySettingsMethodName,
                IsBaselineMethod = route.IsBaselineMethod
            });

        if (contract.CrossPlanDecisionAnchorPrimaryFieldFocuses.Count != 4
            || contract.AlgorithmFamilyDecisionAnchorPrimaryFieldFocuses.Count != 4
            || contract.CrossPlanRequiredResultPrimaryFieldFocuses.Count != 18
            || contract.AlgorithmFamilyRequiredResultPrimaryFieldFocuses.Count != 18)
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: single-plan contract aggregation defaults mismatch. anchorCrossPlanCount={contract.CrossPlanDecisionAnchorPrimaryFieldFocuses.Count}, anchorFamilyCount={contract.AlgorithmFamilyDecisionAnchorPrimaryFieldFocuses.Count}, resultCrossPlanCount={contract.CrossPlanRequiredResultPrimaryFieldFocuses.Count}, resultFamilyCount={contract.AlgorithmFamilyRequiredResultPrimaryFieldFocuses.Count}");
        }

        if (contract.FormDependencyEvidences.Count != 2)
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: single-plan contract form dependency evidence projection mismatch. actual={contract.FormDependencyEvidences.Count}");
        }

        var snapshotService = new StpDbSnapshotQueryService();
        var plans = snapshotService.ListMotorYMethodAdaptationPlans();
        var loadBPlan = plans.FirstOrDefault(x => string.Equals(x.CanonicalCode, MotorYTestMethodCodes.LoadB, StringComparison.Ordinal))
            ?? throw new InvalidOperationException("Motor_Y legacy algorithm dependency smoke test failed: missing LoadB adaptation plan from stp.db snapshot.");


        var loadBFailures = new List<string>();
        if (!loadBPlan.RequiresRatedParams)
        {
            loadBFailures.Add("RequiresRatedParams=false");
        }

        if (!string.Equals(loadBPlan.RecommendedLegacyCode, "B法负载试验", StringComparison.Ordinal))
        {
            loadBFailures.Add($"RecommendedLegacyCode='{loadBPlan.RecommendedLegacyCode}'");
        }

        if (loadBPlan.RecommendedLegacyCodeCount != 265)
        {
            loadBFailures.Add($"RecommendedLegacyCodeCount={loadBPlan.RecommendedLegacyCodeCount}");
        }

        if (Math.Abs(loadBPlan.RecommendedLegacyCodeShare - 1d) > 0.0001d)
        {
            loadBFailures.Add($"RecommendedLegacyCodeShare={loadBPlan.RecommendedLegacyCodeShare}");
        }

        if (loadBPlan.LegacyCodeDistributions.Count != 1)
        {
            loadBFailures.Add($"LegacyCodeDistributions.Count={loadBPlan.LegacyCodeDistributions.Count}");
        }

        if (!string.Equals(loadBPlan.LegacyCodeSelectionSummary, "recommended legacy code 'B法负载试验' for MotorY.LoadB (265/265, 100pp); legacy code variants stable for MotorY.LoadB: only 'B法负载试验' observed", StringComparison.Ordinal))
        {
            loadBFailures.Add($"LegacyCodeSelectionSummary='{loadBPlan.LegacyCodeSelectionSummary}'");
        }

        if (!loadBPlan.RequiredRatedParamFields.SequenceEqual(new[] { "GB" }, StringComparer.Ordinal))
        {
            loadBFailures.Add($"RequiredRatedParamFields=[{string.Join(",", loadBPlan.RequiredRatedParamFields)}]");
        }

        if (!loadBPlan.RequiredResultFields.SequenceEqual(new[] { "A", "B", "R", "Pcu1", "Pcu2", "θs", "ResultDataList" }, StringComparer.Ordinal))
        {
            loadBFailures.Add($"RequiredResultFields=[{string.Join(",", loadBPlan.RequiredResultFields)}]");
        }

        if (!loadBPlan.RequiredIntermediateResultFields.SequenceEqual(new[] { "R1t", "Pcu1t", "Nst", "St", "Ub", "Pfe", "Pcu2t", "Tx", "P2tx", "Pl", "A", "B", "R", "Ps", "θs" }, StringComparer.Ordinal))
        {
            loadBFailures.Add($"RequiredIntermediateResultFields=[{string.Join(",", loadBPlan.RequiredIntermediateResultFields)}]");
        }

        if (loadBPlan.CoveredRequiredIntermediateResultFieldCount != 15)
        {
            loadBFailures.Add($"CoveredRequiredIntermediateResultFieldCount={loadBPlan.CoveredRequiredIntermediateResultFieldCount}");
        }

        if (loadBPlan.MissingRequiredIntermediateResultFieldCount != 0)
        {
            loadBFailures.Add($"MissingRequiredIntermediateResultFieldCount={loadBPlan.MissingRequiredIntermediateResultFieldCount}");
        }

        if (!string.Equals(loadBPlan.RequiredIntermediateResultFieldCoverageSummary, "result required fields covered 15/15 (100pp); missing: none", StringComparison.Ordinal))
        {
            loadBFailures.Add($"RequiredIntermediateResultFieldCoverageSummary='{loadBPlan.RequiredIntermediateResultFieldCoverageSummary}'");
        }

        if (!loadBPlan.UpstreamDependenciesSatisfied)
        {
            loadBFailures.Add("UpstreamDependenciesSatisfied=false");
        }

        if (!string.Equals(loadBPlan.UpstreamDependencySummary, "upstream dependencies satisfied (MotorY.NoLoad + MotorY.HeatRun); observed 2/2 required upstream codes; observed legacy upstream aliases: MotorY.NoLoad=[空载特性完全试验|空载特性测量|空载特性试验|空载试验|空载试验（出厂）|陪试空载特性试验], MotorY.HeatRun=[温度计法热试验|热试验|热试验2|陪试热试验]", StringComparison.Ordinal))
        {
            loadBFailures.Add($"UpstreamDependencySummary='{loadBPlan.UpstreamDependencySummary}'");
        }

        if (!string.Equals(loadBPlan.RawDataSignalCoverageSummary, $"raw data signals covered 8/8 (100pp); raw samples={loadBPlan.RawDataSampleCount}; missing: none; observed: Frequency, I1, Nt, P1t, Tt, U, θ1t, θa", StringComparison.Ordinal))
        {
            loadBFailures.Add($"RawDataSignalCoverageSummary='{loadBPlan.RawDataSignalCoverageSummary}'");
        }

        if (!string.Equals(loadBPlan.LegacyAlgorithmRulesObservedPayloadSummary, "legacy algorithm rule observed payload fields observed 9/9 (100pp); missing: none; observed: A, B, GB, Ps, R, ResultDataList, θb, θs, θw", StringComparison.Ordinal))
        {
            loadBFailures.Add($"LegacyAlgorithmRulesObservedPayloadSummary='{loadBPlan.LegacyAlgorithmRulesObservedPayloadSummary}'");
        }

        if (loadBPlan.CoveredLegacyDecisionAnchorCount != 3)
        {
            loadBFailures.Add($"CoveredLegacyDecisionAnchorCount={loadBPlan.CoveredLegacyDecisionAnchorCount}");
        }

        if (loadBPlan.MissingLegacyDecisionAnchorCount != 0)
        {
            loadBFailures.Add($"MissingLegacyDecisionAnchorCount={loadBPlan.MissingLegacyDecisionAnchorCount}");
        }

        if (Math.Abs(loadBPlan.LegacyDecisionAnchorCoverageRatio - 1d) > 0.0001d)
        {
            loadBFailures.Add($"LegacyDecisionAnchorCoverageRatio={loadBPlan.LegacyDecisionAnchorCoverageRatio}");
        }

        if (loadBPlan.LegacyDecisionAnchorCoveragePercentagePoints != 100)
        {
            loadBFailures.Add($"LegacyDecisionAnchorCoveragePercentagePoints={loadBPlan.LegacyDecisionAnchorCoveragePercentagePoints}");
        }

        if (!loadBPlan.LegacyDecisionAnchorsBackedByObservedPayload)
        {
            loadBFailures.Add("LegacyDecisionAnchorsBackedByObservedPayload=false");
        }

        if (!string.Equals(loadBPlan.LegacyDecisionAnchorSummary, "legacy decision anchors covered 3/3 (100pp); missing: none", StringComparison.Ordinal))
        {
            loadBFailures.Add($"LegacyDecisionAnchorSummary='{loadBPlan.LegacyDecisionAnchorSummary}'");
        }

        if (loadBFailures.Count > 0)
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: stp.db adaptation plan dependency projection mismatch for LoadB. failures=[{string.Join(" | ", loadBFailures)}], actual decisionAnchorCovered={loadBPlan.CoveredLegacyDecisionAnchorCount}, missing={loadBPlan.MissingLegacyDecisionAnchorCount}, ratio={loadBPlan.LegacyDecisionAnchorCoverageRatio}, pp={loadBPlan.LegacyDecisionAnchorCoveragePercentagePoints}, backed={loadBPlan.LegacyDecisionAnchorsBackedByObservedPayload}, gaps=[{string.Join(" | ", loadBPlan.LegacyDecisionAnchorsObservedPayloadGaps.Select(gap => $"{gap.SignalOrRule}:{gap.CoveredByObservedPayload}:{string.Join(",", gap.ObservedPayloadFields)}:{string.Join(",", gap.MissingPayloadFields)}"))}], summary='{loadBPlan.LegacyDecisionAnchorSummary}'");
        }

        var bucketMap = loadBPlan.DependencyBuckets.ToDictionary(x => x.BucketKey, StringComparer.Ordinal);
        var expectedBucketKeys = new[]
        {
            "upstream",
            "payload-fields",
            "rated-params",
            "result-fields",
            "intermediate-result-fields",
            "raw-data-signals",
            "structured-payload-signals",
            "structured-result-signals",
            "formula-signals",
            "legacy-rules",
            "legacy-decision-anchors",
            "legacy-decision-anchor-resolutions",
            "legacy-decision-anchor-fields"
        };

        if (bucketMap.Count != 13
            || !expectedBucketKeys.All(bucketMap.ContainsKey)
            || !bucketMap.TryGetValue("upstream", out var upstreamBucket)
            || upstreamBucket.RequiredCount != 2
            || upstreamBucket.CoveredCount != 2
            || upstreamBucket.MissingCount != 0
            || !bucketMap.TryGetValue("payload-fields", out var payloadBucket)
            || payloadBucket.RequiredCount != 13
            || payloadBucket.CoveredCount != 13
            || payloadBucket.MissingCount != 0
            || payloadBucket.MissingItems.Count != 0
            || !bucketMap.TryGetValue("rated-params", out var ratedBucket)
            || ratedBucket.RequiredCount != 1
            || ratedBucket.CoveredCount != 1
            || ratedBucket.MissingCount != 0
            || !bucketMap.TryGetValue("result-fields", out var resultBucket)
            || resultBucket.RequiredCount != 7
            || resultBucket.CoveredCount != 7
            || resultBucket.MissingCount != 0
            || resultBucket.MissingItems.Count != 0
            || !bucketMap.TryGetValue("intermediate-result-fields", out var intermediateBucket)
            || intermediateBucket.RequiredCount != 15
            || intermediateBucket.CoveredCount != 15
            || intermediateBucket.MissingCount != 0
            || intermediateBucket.MissingItems.Count != 0
            || !bucketMap.TryGetValue("raw-data-signals", out var rawBucket)
            || rawBucket.RequiredCount != 8
            || rawBucket.CoveredCount != 8
            || rawBucket.MissingCount != 0
            || rawBucket.MissingItems.Count != 0
            || !bucketMap.TryGetValue("structured-payload-signals", out var structuredPayloadBucket)
            || structuredPayloadBucket.RequiredCount != 11
            || structuredPayloadBucket.CoveredCount != 11
            || structuredPayloadBucket.MissingCount != 0
            || structuredPayloadBucket.MissingItems.Count != 0
            || !bucketMap.TryGetValue("structured-result-signals", out var structuredResultBucket)
            || structuredResultBucket.RequiredCount != 7
            || structuredResultBucket.CoveredCount != 7
            || structuredResultBucket.MissingCount != 0
            || !bucketMap.TryGetValue("formula-signals", out var formulaBucket)
            || formulaBucket.RequiredCount != 5
            || formulaBucket.CoveredCount != 5
            || formulaBucket.MissingCount != 0
            || formulaBucket.MissingItems.Count != 0
            || !bucketMap.TryGetValue("legacy-rules", out var ruleBucket)
            || ruleBucket.RequiredCount != 9
            || ruleBucket.CoveredCount != 9
            || ruleBucket.MissingCount != 0
            || ruleBucket.MissingItems.Count != 0
            || !bucketMap.TryGetValue("legacy-decision-anchors", out var decisionAnchorBucket)
            || decisionAnchorBucket.RequiredCount != 3
            || decisionAnchorBucket.CoveredCount != 3
            || decisionAnchorBucket.MissingCount != 0
            || decisionAnchorBucket.MissingItems.Count != 0
            || !bucketMap.TryGetValue("legacy-decision-anchor-resolutions", out var resolutionBucket)
            || resolutionBucket.RequiredCount != 4
            || resolutionBucket.CoveredCount != 4
            || resolutionBucket.MissingCount != 0
            || resolutionBucket.MissingItems.Count != 0
            || !bucketMap.TryGetValue("legacy-decision-anchor-fields", out var decisionAnchorFieldBucket)
            || decisionAnchorFieldBucket.RequiredCount != 12
            || decisionAnchorFieldBucket.CoveredCount != 12
            || decisionAnchorFieldBucket.MissingCount != 0
            || decisionAnchorFieldBucket.MissingItems.Count != 0)
        {
            throw new InvalidOperationException($"Motor_Y legacy algorithm dependency smoke test failed: dependency bucket projection mismatch for LoadB. bucketKeys=[{string.Join(",", bucketMap.Keys.OrderBy(x => x, StringComparer.Ordinal))}], buckets=[{string.Join(" | ", loadBPlan.DependencyBuckets.Select(bucket => $"{bucket.BucketKey}:{bucket.RequiredCount}:{bucket.CoveredCount}:{bucket.MissingCount}:{string.Join(",", bucket.MissingItems)}:{bucket.Summary}"))}], structuredPayloadObserved=[{string.Join(",", loadBPlan.ObservedStructuredPayloadSignals)}], structuredPayloadMissing=[{string.Join(",", loadBPlan.MissingStructuredPayloadSignals)}], structuredPayloadSummary='{loadBPlan.StructuredPayloadSignalCoverageSummary}'");
        }
    }
}
