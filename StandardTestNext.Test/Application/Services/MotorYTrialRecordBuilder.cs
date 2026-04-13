using System.Text.Json;
using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYTrialRecordBuilder
{
    public IReadOnlyList<TestRecordItemAggregate> BuildTrialItems(
        MotorRatedParamsContract rated,
        IReadOnlyCollection<MotorRealtimeSampleContract> samples,
        int noLoadRConverseType = 0)
    {
        var orderedSamples = samples.OrderBy(sample => sample.SampleTime).ToArray();
        var keyPointSamples = orderedSamples.Where(sample => sample.IsRecordPoint).ToArray();
        var continuousSamples = orderedSamples.Where(sample => !sample.IsRecordPoint).ToArray();

        var items = new List<TestRecordItemAggregate>
        {
            BuildDcResistanceItem(rated),
            BuildNoLoadItem(rated, orderedSamples, noLoadRConverseType),
            BuildHeatRunItem(rated, orderedSamples),
            BuildLoadAItem(rated, keyPointSamples, orderedSamples),
            BuildLoadBItem(rated, continuousSamples, orderedSamples),
            BuildLockedRotorItem(rated, keyPointSamples, orderedSamples)
        };

        return items;
    }

    private static TestRecordItemAggregate ApplyBaselineProfile(TestRecordItemAggregate item, string canonicalCode)
    {
        MotorYTrialItemProfileCatalog.ApplyBaseline(item, canonicalCode);
        return item;
    }

    private static TestRecordItemAggregate BuildDcResistanceItem(MotorRatedParamsContract rated)
    {
        var ruv = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.118, 4);
        var rvw = Math.Round(ruv * 1.006, 4);
        var rwu = Math.Round(ruv * 0.997, 4);
        var r1 = Math.Round((ruv + rvw + rwu) / 3, 4);
        var deltaR = Math.Round((new[] { ruv, rvw, rwu }.Max() - new[] { ruv, rvw, rwu }.Min()) / Math.Max(r1, 0.0001) * 100, 4);
        var convertedR = Math.Round(r1 * (235 + 25) / (235 + 26.5), 4);

        return ApplyBaselineProfile(new TestRecordItemAggregate
        {
            ItemCode = "MotorY.DcResistance",
            MethodCode = MotorYTestMethodCodes.DcResistance,
            DataJson = JsonSerializer.Serialize(new
            {
                Ruv = ruv,
                Rvw = rvw,
                Rwu = rwu,
                R1 = r1,
                R1c = r1,
                θ1c = 26.5,
                ΔR = deltaR,
                R = convertedR,
                Connection = rated.Connection,
                IsAnalysis = false
            }),
            Remark = "Motor_Y 直流电阻试验骨架数据（旧 TestData 结构对齐版）。"
        }, MotorYTestMethodCodes.DcResistance);
    }

    private static TestRecordItemAggregate BuildNoLoadItem(
        MotorRatedParamsContract rated,
        IReadOnlyList<MotorRealtimeSampleContract> samples,
        int rConverseType)
    {
        var r1c = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.12, 4);
        const double theta1c = 26.5;
        const double k1 = 235.0;
        const int order = 3;
        const int decimalPlaces = 4;

        var dataList = samples
            .Select((sample, index) =>
            {
                var theta0 = Math.Round(theta1c + index * 0.6, decimalPlaces);
                var u0DivideUn = rated.RatedVoltage == 0 ? 0 : sample.VoltageAverage / rated.RatedVoltage;
                var r0 = Math.Round(r1c * (k1 + theta0) / (k1 + theta1c), decimalPlaces);
                var p0cu1 = Math.Round(1.5 * r0 * sample.CurrentAverage * sample.CurrentAverage, decimalPlaces);
                var pcon = Math.Round(sample.Power - p0cu1, decimalPlaces);
                var i01 = Math.Round(sample.CurrentAverage * 0.995, decimalPlaces);
                var i02 = Math.Round(sample.CurrentAverage * 1.002, decimalPlaces);
                var i03 = Math.Round(sample.CurrentAverage * 1.003, decimalPlaces);
                var i0Average = (i01 + i02 + i03) / 3d;
                var deltaI0 = i0Average <= 0d
                    ? 0d
                    : Math.Round(new[]
                    {
                        Math.Abs(i01 - i0Average),
                        Math.Abs(i02 - i0Average),
                        Math.Abs(i03 - i0Average)
                    }.Max() / i0Average * 100d, decimalPlaces);
                return new
                {
                    U0 = Math.Round(sample.VoltageAverage, decimalPlaces),
                    U0DivideUn = Math.Round(u0DivideUn, decimalPlaces),
                    U0DivideUnSquare = Math.Round(Math.Pow(u0DivideUn, 2), decimalPlaces),
                    I0 = Math.Round(sample.CurrentAverage, decimalPlaces),
                    I01 = i01,
                    I02 = i02,
                    I03 = i03,
                    P0 = Math.Round(sample.Power, decimalPlaces),
                    Cosφ = rated.RatedPower == 0 ? 0 : Math.Round(Math.Min(0.98, sample.Power / Math.Max(sample.VoltageAverage * sample.CurrentAverage * 1.732 / 1000.0, 0.0001)), decimalPlaces),
                    Frequency = Math.Round(sample.Frequency, decimalPlaces),
                    θ0 = theta0,
                    R0 = r0,
                    ΔI0 = deltaI0,
                    P0cu1 = p0cu1,
                    Pcon = pcon,
                    Pfe = Math.Round(pcon * 0.82, decimalPlaces),
                    n0 = Math.Round(sample.Speed, decimalPlaces),
                    T0 = Math.Round(sample.Torque, decimalPlaces)
                };
            })
            .ToArray();

        var computation = MotorYNoLoadComputation.Compute(
            dataList.Select(x => new MotorYNoLoadComputedPoint
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
            rated.RatedVoltage,
            order,
            decimalPlaces,
            r1c,
            theta1c,
            k1,
            rConverseType);
        var adjustedDataList = computation.AdjustedPoints
            .Select((point, index) => new
            {
                U0 = point.U0,
                U0DivideUn = point.U0DivideUn,
                U0DivideUnSquare = point.U0DivideUnSquare,
                I0 = point.I0,
                I01 = dataList[index].I01,
                I02 = dataList[index].I02,
                I03 = dataList[index].I03,
                P0 = point.P0,
                Cosφ = dataList[index].Cosφ,
                Frequency = dataList[index].Frequency,
                θ0 = point.Theta0,
                R0 = point.R0,
                ΔI0 = point.DeltaI0,
                P0cu1 = point.P0cu1,
                Pcon = point.Pcon,
                Pfe = Math.Round(Math.Max(0d, point.Pcon - computation.Pfw), decimalPlaces),
                n0 = dataList[index].n0,
                T0 = dataList[index].T0
            })
            .ToArray();
        var ratedPoint = computation.RatedPoint;
        var pfwFitSamples = computation.PfwFitSamples;
        var pfwFitWindowReady = computation.PfwFitWindowReady;
        var pfw = computation.Pfw;
        var coefficientOfPfe = computation.CoefficientOfPfe;
        var pfe = computation.Pfe;
        var fittedI0AtRated = computation.FittedI0AtRated;
        var fittedDeltaI0AtRated = computation.FittedDeltaI0AtRated;
        var fittedP0AtRated = computation.FittedP0AtRated;
        var fittedPcuAtRated = computation.FittedPcuAtRated;

        return ApplyBaselineProfile(new TestRecordItemAggregate
        {
            ItemCode = "MotorY.NoLoad",
            MethodCode = MotorYTestMethodCodes.NoLoad,
            DataJson = JsonSerializer.Serialize(new
            {
                Un = rated.RatedVoltage,
                R1c = r1c,
                θ1c = theta1c,
                K1 = k1,
                Order = order,
                DecimalPlaces = decimalPlaces,
                DataList = adjustedDataList,
                P0 = fittedP0AtRated,
                I0 = fittedI0AtRated,
                ΔI0 = fittedDeltaI0AtRated,
                Pcu = fittedPcuAtRated,
                Pfw = pfw,
                Pfe = pfe,
                θ0 = computation.ComputedTheta0,
                R0 = computation.ComputedR0,
                CoefficientOfPfe = coefficientOfPfe,
                RConverseType = rConverseType,
                IsAnalysis = false,
                U0DivideUnIsEquesToOne_I0 = fittedI0AtRated,
                U0DivideUnIsEquesToOne_P0 = fittedP0AtRated,
                U0DivideUnIsEquesToOne_Pcu = fittedPcuAtRated,
                U0DivideUnIsEquesToOne_Pfe = pfe,
                U0DivideUnIsEquesToOne_DeltaI0 = fittedDeltaI0AtRated,
                U0DivideUnIsEquesToOne_R0 = computation.ComputedR0,
                U0DivideUnIsEquesToOne_θ0 = computation.ComputedTheta0,
                PfwFitSampleCount = pfwFitSamples.Count,
                PfwFitWindowReady = pfwFitWindowReady
            }),
            Remark = $"Motor_Y 空载试验骨架数据（旧 TestData 结构对齐版；RConverseType 当前由上游 seed/aggregate 输入，当前值={rConverseType}）。"
        }, MotorYTestMethodCodes.NoLoad);
    }

    private static TestRecordItemAggregate BuildHeatRunItem(
        MotorRatedParamsContract rated,
        IReadOnlyList<MotorRealtimeSampleContract> samples)
    {
        var data1List = samples
            .Select((sample, index) => new
            {
                Time = sample.SampleTime,
                U = Math.Round(sample.VoltageAverage, 4),
                Iavg = Math.Round(sample.CurrentAverage, 4),
                Iu = Math.Round(sample.CurrentAverage * 0.995, 4),
                Iv = Math.Round(sample.CurrentAverage * 1.002, 4),
                Iw = Math.Round(sample.CurrentAverage * 1.003, 4),
                P1 = Math.Round(sample.Power, 4),
                P2 = Math.Round(sample.Power * 0.91, 4),
                Td = Math.Round(sample.Torque, 4),
                N = Math.Round(sample.Speed, 4),
                θb = 26.5,
                θ1 = Math.Round(58 + index * 1.6, 4),
                Cosφ = 0.86,
                Freq = Math.Round(sample.Frequency, 4),
                η = 0.91,
                Temp1 = Math.Round(58 + index * 1.2, 4),
                Temp2 = Math.Round(58.4 + index * 1.2, 4),
                Temp3 = Math.Round(58.8 + index * 1.2, 4),
                Temp4 = Math.Round(59.2 + index * 1.2, 4),
                Temp5 = Math.Round(59.6 + index * 1.2, 4),
                Temp6 = Math.Round(60.0 + index * 1.2, 4),
                Temp7 = Math.Round(60.4 + index * 1.2, 4),
                Temp8 = Math.Round(60.8 + index * 1.2, 4),
                Temp9 = Math.Round(61.2 + index * 1.2, 4),
                Temp10 = Math.Round(61.6 + index * 1.2, 4),
                Temp11 = Math.Round(62.0 + index * 1.2, 4),
                Temp12 = Math.Round(62.4 + index * 1.2, 4),
                Temp13 = Math.Round(62.8 + index * 1.2, 4),
                Temp14 = Math.Round(63.2 + index * 1.2, 4),
                Temp15 = Math.Round(63.6 + index * 1.2, 4),
                Temp16 = Math.Round(64.0 + index * 1.2, 4),
                AmbientTemperature = 26.5
            })
            .ToArray();

        var data2List = new[]
        {
            new { Time = 0.5, R = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.129, 4) },
            new { Time = 1.0, R = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.131, 4) },
            new { Time = 2.0, R = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.134, 4) }
        };

        return ApplyBaselineProfile(new TestRecordItemAggregate
        {
            ItemCode = "MotorY.HeatRun",
            MethodCode = MotorYTestMethodCodes.HeatRun,
            DataJson = JsonSerializer.Serialize(new
            {
                Rc = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.12, 4),
                θc = 26.5,
                K1 = 235.0,
                Rn = data2List.First().R,
                θb = 26.5,
                Time = 2.0,
                θs = 92.5,
                Order = 4,
                DecimalPlaces = 4,
                Data1List = data1List,
                Data2List = data2List,
                θw = 88.6,
                Rws = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.136, 4),
                Rw = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.134, 4),
                Δθ = 62.1,
                Δθn = 58.0,
                Pn = rated.RatedPower,
                HotStateType = 1,
                θbChanelSelect = 0,
                θ1ChanelSelect = 15,
                IsAnalysis = false,
                IsManual = false
            }),
            Remark = "Motor_Y 热试验骨架数据（旧 TestData 结构对齐版）。"
        }, MotorYTestMethodCodes.HeatRun);
    }

    private static TestRecordItemAggregate BuildLoadAItem(
        MotorRatedParamsContract rated,
        IReadOnlyList<MotorRealtimeSampleContract> keyPointSamples,
        IReadOnlyList<MotorRealtimeSampleContract> fallbackSamples)
    {
        var source = keyPointSamples.Count > 0 ? keyPointSamples : fallbackSamples;
        var rawDataList = source
            .Select((sample, index) => new
            {
                U = Math.Round(sample.VoltageAverage, 4),
                I1 = Math.Round(sample.CurrentAverage, 4),
                Iu = Math.Round(sample.CurrentAverage * 0.995, 4),
                Iv = Math.Round(sample.CurrentAverage * 1.002, 4),
                Iw = Math.Round(sample.CurrentAverage * 1.003, 4),
                Nt = Math.Round(sample.Speed, 4),
                Nst = Math.Round(60 * sample.Frequency / Math.Max(rated.PolePairs, 1), 4),
                St = Math.Round((60 * sample.Frequency / Math.Max(rated.PolePairs, 1) - sample.Speed) / Math.Max(60 * sample.Frequency / Math.Max(rated.PolePairs, 1), 0.0001), 6),
                P1t = Math.Round(sample.Power, 4),
                Frequency = Math.Round(sample.Frequency, 4),
                θ1t = Math.Round(55 + index * 1.4, 4),
                R1t = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.125, 4),
                Pcu1t = Math.Round(1.5 * 0.125 * sample.CurrentAverage * sample.CurrentAverage, 4),
                Ub = Math.Round(sample.VoltageAverage * 0.98, 4),
                Pfe = Math.Round(sample.Power * 0.16, 4),
                Pfw = Math.Round(sample.Power * 0.08, 4),
                Pcu2t = Math.Round(sample.Power * 0.11, 4),
                Tt = Math.Round(sample.Torque, 4),
                Tx = Math.Round(sample.Torque * 1.01, 4),
                P2tx = Math.Round(sample.Power * 0.9, 4),
                Pcu1x = Math.Round(sample.Power * 0.09, 4),
                ΔPcu1 = Math.Round(sample.Power * 0.01, 4),
                Sx = Math.Round((60 * sample.Frequency / Math.Max(rated.PolePairs, 1) - sample.Speed) / Math.Max(60 * sample.Frequency / Math.Max(rated.PolePairs, 1), 0.0001), 6),
                Pcu2x = Math.Round(sample.Power * 0.1, 4),
                ΔPcu2 = Math.Round(sample.Power * 0.01, 4),
                Nx = Math.Round(sample.Speed * 0.998, 4),
                P1x = Math.Round(sample.Power * 1.002, 4),
                P2x = Math.Round(sample.Power * 0.902, 4),
                η = 0.902,
                Cosφ = 0.87
            })
            .ToArray();

        var resultDataList = rawDataList
            .Select((row, index) => new
            {
                P1 = row.P1x,
                P2 = row.P2x,
                I1 = row.I1,
                η = row.η,
                Cosφ = row.Cosφ,
                S = row.Sx,
                Percentage = Math.Round((index + 1) * 100.0 / Math.Max(rawDataList.Length, 1), 2)
            })
            .ToArray();

        return ApplyBaselineProfile(new TestRecordItemAggregate
        {
            ItemCode = "MotorY.LoadA",
            MethodCode = MotorYTestMethodCodes.LoadA,
            DataJson = JsonSerializer.Serialize(new
            {
                Un = rated.RatedVoltage,
                Pn = rated.RatedPower,
                R1c = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.12, 4),
                θ1c = 26.5,
                K1 = 235.0,
                K2 = 235.0,
                θa = 26.5,
                ΔT = 0.35,
                Pfw = 35.0,
                Order = 3,
                PolePairs = rated.PolePairs,
                DecimalPlaces = 2,
                CoefficientOfPfe = new[] { 0.0, 0.12, 0.03, 0.005 },
                RawDataList = rawDataList,
                ResultDataList = resultDataList,
                CorrectionType = 1,
                IsAnalysis = false
            }),
            Remark = "Motor_Y A法负载试验骨架数据（旧 TestData 结构对齐版）。"
        }, MotorYTestMethodCodes.LoadA);
    }

    private static TestRecordItemAggregate BuildLoadBItem(
        MotorRatedParamsContract rated,
        IReadOnlyList<MotorRealtimeSampleContract> continuousSamples,
        IReadOnlyList<MotorRealtimeSampleContract> fallbackSamples)
    {
        var source = continuousSamples.Count > 0 ? continuousSamples : fallbackSamples;
        var rawDataList = source
            .Select((sample, index) => new
            {
                U = Math.Round(sample.VoltageAverage, 4),
                I1 = Math.Round(sample.CurrentAverage, 4),
                Iu = Math.Round(sample.CurrentAverage * 0.995, 4),
                Iv = Math.Round(sample.CurrentAverage * 1.002, 4),
                Iw = Math.Round(sample.CurrentAverage * 1.003, 4),
                Nt = Math.Round(sample.Speed, 4),
                Nst = Math.Round(60 * sample.Frequency / Math.Max(rated.PolePairs, 1), 4),
                St = Math.Round((60 * sample.Frequency / Math.Max(rated.PolePairs, 1) - sample.Speed) / Math.Max(60 * sample.Frequency / Math.Max(rated.PolePairs, 1), 0.0001), 6),
                P1t = Math.Round(sample.Power, 4),
                Frequency = Math.Round(sample.Frequency, 4),
                θ1t = Math.Round(62 + index * 1.1, 4),
                θa = 26.5,
                R1t = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.126, 4),
                Pcu1t = Math.Round(1.5 * 0.126 * sample.CurrentAverage * sample.CurrentAverage, 4),
                Ub = Math.Round(sample.VoltageAverage * 0.98, 4),
                Pfe = Math.Round(sample.Power * 0.16, 4),
                Pfw = Math.Round(sample.Power * 0.08, 4),
                Pcu2t = Math.Round(sample.Power * 0.11, 4),
                Tt = Math.Round(sample.Torque, 4),
                Tx = Math.Round(sample.Torque * 1.01, 4),
                P2tx = Math.Round(sample.Power * 0.9, 4),
                Pl = Math.Round(sample.Power * 0.1, 4),
                θs = Math.Round(85 + index * 0.5, 4),
                Ps = Math.Round(sample.Power * 0.02, 4),
                Rs = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.131, 4),
                Pcu1x = Math.Round(sample.Power * 0.09, 4),
                Sx = Math.Round((60 * sample.Frequency / Math.Max(rated.PolePairs, 1) - sample.Speed) / Math.Max(60 * sample.Frequency / Math.Max(rated.PolePairs, 1), 0.0001), 6),
                Pcu2x = Math.Round(sample.Power * 0.1, 4),
                Nx = Math.Round(sample.Speed * 0.998, 4),
                PT = Math.Round(sample.Power * 0.12, 4),
                P2x = Math.Round(sample.Power * 0.88, 4),
                η = 0.88,
                Cosφ = 0.86,
                Pm = Math.Round(sample.Power * 0.9, 4),
                Temp1 = 68.1,
                Temp2 = 68.3,
                Temp3 = 68.6,
                Temp4 = 68.8,
                Temp5 = 69.0,
                Temp6 = 69.2,
                Temp7 = 69.4,
                Temp8 = 69.6,
                Temp9 = 69.8,
                Temp10 = 70.0,
                Temp11 = 70.2,
                Temp12 = 70.4,
                Temp13 = 70.6,
                Temp14 = 70.8,
                Temp15 = 71.0,
                Temp16 = 71.2,
                AmbientTemperature = 26.5
            })
            .ToArray();

        var resultDataList = rawDataList
            .Select((row, index) => new
            {
                P1 = row.P1t,
                P2 = row.P2x,
                I1 = row.I1,
                η = row.η,
                Cosφ = row.Cosφ,
                S = row.Sx,
                Percentage = Math.Round((index + 1) * 100.0 / Math.Max(rawDataList.Length, 1), 2),
                Pcu1x = row.Pcu1x,
                Pcu2x = row.Pcu2x,
                Ps = row.Ps
            })
            .ToArray();

        var loadBPayload = JsonSerializer.Serialize(new
        {
            Un = rated.RatedVoltage,
            Pn = rated.RatedPower,
            K1 = 235.0,
            K2 = 225.0,
            PolePairs = rated.PolePairs,
            Order = 3,
            DecimalPlaces = 2,
            TorqueCorrection = false,
            R1c = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.12, 4),
            θ1c = 26.5,
            θw = 88.6,
            θb = 26.5,
            θs = 92.5,
            ΔT = 0.35,
            A = 1.12,
            B = 0.08,
            R = 0.97,
            Pfw = 35.0,
            Pcu1 = 1.8,
            Pcu2 = 1.4,
            Ps = 0.6,
            CoefficientOfPfe = new[] { 0.0, 0.12, 0.03, 0.005 },
            RawDataList = rawDataList,
            ResultDataList = resultDataList,
            IsAnalysis = false,
            IsTorqueModify = false,
            θ1tChanelSelect = 15,
            θaChanelSelect = 0
        });

        using var loadBPayloadDocument = JsonDocument.Parse(loadBPayload);
        var loadBPayloadMap = loadBPayloadDocument.RootElement
            .EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.Clone());
        loadBPayloadMap["bad-point-refit"] = JsonSerializer.SerializeToElement(true);
        loadBPayloadMap["ratios"] = JsonSerializer.SerializeToElement(new[] { 0.25, 0.5, 0.75, 1.0 });
        loadBPayloadMap["cuC"] = JsonSerializer.SerializeToElement(0.84);

        return ApplyBaselineProfile(new TestRecordItemAggregate
        {
            ItemCode = "MotorY.LoadB",
            MethodCode = MotorYTestMethodCodes.LoadB,
            DataJson = JsonSerializer.Serialize(loadBPayloadMap),
            Remark = "Motor_Y B法负载试验骨架数据（旧 TestData 结构对齐版）。"
        }, MotorYTestMethodCodes.LoadB);
    }

    private static TestRecordItemAggregate BuildLockedRotorItem(
        MotorRatedParamsContract rated,
        IReadOnlyList<MotorRealtimeSampleContract> keyPointSamples,
        IReadOnlyList<MotorRealtimeSampleContract> fallbackSamples)
    {
        var source = keyPointSamples.Count > 0 ? keyPointSamples : fallbackSamples;
        var dataList = source
            .Select(sample => new
            {
                Uk = Math.Round(sample.VoltageAverage * 0.32, 4),
                Ik = Math.Round(sample.CurrentAverage * 1.85, 4),
                Pk = Math.Round(sample.Power * 1.35, 4),
                Tk = Math.Round(sample.Torque * 1.22, 4),
                Pkcu1 = Math.Round(sample.Power * 0.28, 4),
                Pfe = Math.Round(sample.Power * 0.06, 4),
                ns = Math.Round(60 * sample.Frequency / Math.Max(rated.PolePairs, 1), 4),
                θ = 26.5,
                R = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.12, 4),
                Frequency = Math.Round(sample.Frequency, 4),
                θ1s = 25.0,
                UkDivideUn = rated.RatedVoltage == 0 ? 0 : Math.Round(sample.VoltageAverage * 0.32 / rated.RatedVoltage, 4)
            })
            .ToArray();

        return ApplyBaselineProfile(new TestRecordItemAggregate
        {
            ItemCode = "MotorY.LockedRotor",
            MethodCode = MotorYTestMethodCodes.LockedRotor,
            DataJson = JsonSerializer.Serialize(new
            {
                Un = rated.RatedVoltage,
                In = rated.RatedCurrent,
                Tn = rated.RatedSpeed == 0 ? 0 : Math.Round(rated.RatedPower * 9549 / rated.RatedSpeed, 4),
                PolePairs = rated.PolePairs,
                CoefficientOfPfe = new[] { 0.0, 0.12, 0.03, 0.005 },
                K1 = 235.0,
                R1c = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.12, 4),
                θ1c = 26.5,
                Order = 3,
                DecimalPlaces = 2,
                DataList = dataList,
                Ikn = dataList.Length == 0 ? 0 : dataList.Last().Ik,
                IknDivideIn = dataList.Length == 0 || rated.RatedCurrent == 0 ? 0 : Math.Round(dataList.Last().Ik / rated.RatedCurrent, 4),
                Pkn = dataList.Length == 0 ? 0 : dataList.Last().Pk,
                Tkn = dataList.Length == 0 ? 0 : dataList.Last().Tk,
                TknDivideTn = dataList.Length == 0 || rated.RatedSpeed == 0 ? 0 : Math.Round(dataList.Last().Tk / Math.Max(rated.RatedPower * 9549 / rated.RatedSpeed, 0.0001), 4),
                TorqueCalType = 0,
                RCalType = 0,
                C1 = 1.0,
                θw = 88.6,
                θb = 26.5,
                R1s = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.129, 4),
                IsAnalysis = false
            }),
            Remark = "Motor_Y 堵转试验骨架数据（旧 TestData 结构对齐版）。"
        }, MotorYTestMethodCodes.LockedRotor);
    }
}
