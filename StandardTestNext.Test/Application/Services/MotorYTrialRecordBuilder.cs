using System.Text.Json;
using StandardTestNext.Contracts;
using StandardTestNext.Test.Domain.Records;

namespace StandardTestNext.Test.Application.Services;

public sealed class MotorYTrialRecordBuilder
{
    public IReadOnlyList<TestRecordItemAggregate> BuildTrialItems(
        MotorRatedParamsContract rated,
        IReadOnlyCollection<MotorRealtimeSampleContract> samples)
    {
        var orderedSamples = samples.OrderBy(sample => sample.SampleTime).ToArray();
        var keyPointSamples = orderedSamples.Where(sample => sample.IsRecordPoint).ToArray();
        var continuousSamples = orderedSamples.Where(sample => !sample.IsRecordPoint).ToArray();

        var items = new List<TestRecordItemAggregate>
        {
            BuildDcResistanceItem(rated),
            BuildNoLoadItem(rated, orderedSamples),
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
        IReadOnlyList<MotorRealtimeSampleContract> samples)
    {
        var dataList = samples
            .Select(sample => new
            {
                U0 = Math.Round(sample.VoltageAverage, 4),
                U0DivideUn = rated.RatedVoltage == 0 ? 0 : Math.Round(sample.VoltageAverage / rated.RatedVoltage, 4),
                U0DivideUnSquare = rated.RatedVoltage == 0 ? 0 : Math.Round(Math.Pow(sample.VoltageAverage / rated.RatedVoltage, 2), 4),
                I0 = Math.Round(sample.CurrentAverage, 4),
                I01 = Math.Round(sample.CurrentAverage * 0.995, 4),
                I02 = Math.Round(sample.CurrentAverage * 1.002, 4),
                I03 = Math.Round(sample.CurrentAverage * 1.003, 4),
                P0 = Math.Round(sample.Power, 4),
                Cosφ = rated.RatedPower == 0 ? 0 : Math.Round(Math.Min(0.98, sample.Power / Math.Max(sample.VoltageAverage * sample.CurrentAverage * 1.732 / 1000.0, 0.0001)), 4),
                Frequency = Math.Round(sample.Frequency, 4),
                θ0 = 26.5,
                R0 = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.12, 4),
                ΔI0 = 0.8,
                P0cu1 = Math.Round(1.5 * 0.12 * sample.CurrentAverage * sample.CurrentAverage, 4),
                Pcon = Math.Round(sample.Power * 0.82, 4),
                Pfe = Math.Round(sample.Power * 0.55, 4),
                n0 = Math.Round(sample.Speed, 4),
                T0 = Math.Round(sample.Torque, 4)
            })
            .ToArray();

        return ApplyBaselineProfile(new TestRecordItemAggregate
        {
            ItemCode = "MotorY.NoLoad",
            MethodCode = MotorYTestMethodCodes.NoLoad,
            DataJson = JsonSerializer.Serialize(new
            {
                Un = rated.RatedVoltage,
                R1c = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.12, 4),
                θ1c = 26.5,
                K1 = 235.0,
                Order = 3,
                DecimalPlaces = 4,
                DataList = dataList,
                P0 = dataList.Length == 0 ? 0 : dataList.Last().P0,
                I0 = dataList.Length == 0 ? 0 : dataList.Last().I0,
                ΔI0 = dataList.Length == 0 ? 0 : dataList.Last().ΔI0,
                Pcu = dataList.Length == 0 ? 0 : dataList.Last().P0cu1,
                Pfw = dataList.Length == 0 ? 0 : Math.Round(dataList.Last().P0 * 0.18, 4),
                Pfe = dataList.Length == 0 ? 0 : dataList.Last().Pfe,
                θ0 = 26.5,
                R0 = Math.Round((rated.RatedVoltage / Math.Max(rated.RatedCurrent, 1)) * 0.12, 4),
                CoefficientOfPfe = new[] { 0.0, 0.12, 0.03, 0.005 },
                RConverseType = 0,
                IsAnalysis = false,
                U0DivideUnIsEquesToOne_I0 = dataList.Length == 0 ? 0 : dataList.Last().I0,
                U0DivideUnIsEquesToOne_P0 = dataList.Length == 0 ? 0 : dataList.Last().P0,
                U0DivideUnIsEquesToOne_Pcu = dataList.Length == 0 ? 0 : dataList.Last().P0cu1,
                U0DivideUnIsEquesToOne_Pfe = dataList.Length == 0 ? 0 : dataList.Last().Pfe,
                U0DivideUnIsEquesToOne_DeltaI0 = dataList.Length == 0 ? 0 : dataList.Last().ΔI0,
                U0DivideUnIsEquesToOne_R0 = dataList.Length == 0 ? 0 : dataList.Last().R0,
                U0DivideUnIsEquesToOne_θ0 = dataList.Length == 0 ? 0 : dataList.Last().θ0,
                PfwFitSampleCount = dataList.Count(x => x.U0DivideUn < 0.51),
                PfwFitWindowReady = dataList.Any(x => x.U0DivideUn < 0.51)
            }),
            Remark = "Motor_Y 空载试验骨架数据（旧 TestData 结构对齐版）。"
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

        return ApplyBaselineProfile(new TestRecordItemAggregate
        {
            ItemCode = "MotorY.LoadB",
            MethodCode = MotorYTestMethodCodes.LoadB,
            DataJson = JsonSerializer.Serialize(new
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
            }),
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
