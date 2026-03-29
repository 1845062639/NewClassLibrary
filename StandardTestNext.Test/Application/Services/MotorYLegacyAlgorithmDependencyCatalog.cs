namespace StandardTestNext.Test.Application.Services;

/// <summary>
/// 结构化沉淀旧 Motor_Y 算法入口对前置试验数据、额定参数、关键字段与核心公式语义的依赖画像，
/// 供 next-gen 适配计划 / App 提示 / 后续算法 adapter 直接消费。
/// 目标不是现在就重写算法，而是先把“要对齐旧算法至少需要什么输入、会走什么关键计算规则”显式化并锁进闭环。
/// </summary>
public sealed class MotorYLegacyAlgorithmDependencyProfile
{
    public string CanonicalCode { get; init; } = string.Empty;
    public string AlgorithmEntry { get; init; } = string.Empty;
    public bool RequiresRatedParams { get; init; }
    public IReadOnlyList<string> UpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, IReadOnlyList<string>> UpstreamLegacyAliases { get; init; } = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredRatedParamFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredResultFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredIntermediateResultFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingUpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredStructuredPayloadSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredStructuredResultSignals { get; init; } = Array.Empty<string>();
    public int MinimumRawSampleCount { get; init; }
    public int MinimumStructuredPayloadSampleCount { get; init; }
    public int MinimumStructuredResultSampleCount { get; init; }
    public IReadOnlyList<string> FormulaSignals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> LegacyAlgorithmRules { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> LegacyDecisionAnchors { get; init; } = Array.Empty<string>();
    public string Notes { get; init; } = string.Empty;
}

internal sealed class MotorYStructuredListCoverageSnapshot
{
    public int RequiredCount { get; init; }
    public int CoveredCount { get; init; }
    public int MissingCount { get; init; }
    public IReadOnlyList<string> CoveredItems { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingItems { get; init; } = Array.Empty<string>();
    public double CoverageRatio { get; init; }
    public int CoveragePercentagePoints { get; init; }
    public string Summary { get; init; } = string.Empty;
}

public static class MotorYLegacyAlgorithmDependencyCatalog
{
    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildUpstreamLegacyAliases(params string[] canonicalCodes)
        => canonicalCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.Ordinal)
            .ToDictionary(
                code => code,
                code => (IReadOnlyList<string>)MotorYLegacyItemCodeNormalizer.GetLegacyAliases(code),
                StringComparer.Ordinal);

    private static readonly IReadOnlyDictionary<string, MotorYLegacyAlgorithmDependencyProfile> Profiles =
        new Dictionary<string, MotorYLegacyAlgorithmDependencyProfile>(StringComparer.Ordinal)
        {
            [MotorYTestMethodCodes.DcResistance] = new()
            {
                CanonicalCode = MotorYTestMethodCodes.DcResistance,
                AlgorithmEntry = MotorYLegacyAlgorithmEntrypoints.DcResistance,
                RequiresRatedParams = false,
                UpstreamCanonicalCodes = Array.Empty<string>(),
                RequiredPayloadFields = new[] { "Ruv", "Rvw", "Rwu", "R1", "θ1c" },
                RequiredRatedParamFields = Array.Empty<string>(),
                RequiredResultFields = new[] { "R1", "θ1c" },
                RequiredIntermediateResultFields = new[] { "R1", "θ1c" },
                RequiredStructuredPayloadSignals = new[] { "Ruv", "Rvw", "Rwu" },
                RequiredStructuredResultSignals = new[] { "R1", "θ1c" },
                MinimumRawSampleCount = 0,
                MinimumStructuredPayloadSampleCount = 1,
                MinimumStructuredResultSampleCount = 1,
                FormulaSignals = new[]
                {
                    "输出 R1/θ1c 作为空载、热试验等后续算法入口的基础状态量",
                    "三相电阻 Ruv/Rvw/Rwu 归并后形成后续引用的冷态电阻口径"
                },
                LegacyAlgorithmRules = new[]
                {
                    "该项本身更偏采集/整理，不依赖上游试验项",
                    "后续算法默认把这里产出的 R1/θ1c 视为冷态绕组参考量"
                },
                LegacyDecisionAnchors = new[]
                {
                    "无上游试验依赖，可直接作为 Motor_Y 冷态基线入口",
                    "结果字段 R1/θ1c 是否齐备，决定后续空载/热试验能否进入旧算法口径"
                },
                Notes = "旧 FrmMotor_Y_Direct_Current_Resistance 页面直接采集/整理后入库，供空载/热试验继续引用 R1/θ1c。"
            },
            [MotorYTestMethodCodes.NoLoad] = new()
            {
                CanonicalCode = MotorYTestMethodCodes.NoLoad,
                AlgorithmEntry = MotorYLegacyAlgorithmEntrypoints.NoLoad,
                RequiresRatedParams = false,
                UpstreamCanonicalCodes = new[] { MotorYTestMethodCodes.DcResistance },
                UpstreamLegacyAliases = BuildUpstreamLegacyAliases(MotorYTestMethodCodes.DcResistance),
                RequiredPayloadFields = new[] { "DataList", "Un", "R1c", "θ1c", "K1", "Order" },
                RequiredRatedParamFields = Array.Empty<string>(),
                RequiredResultFields = new[] { "I0", "ΔI0", "P0", "Pcu", "Pfw", "Pfe", "CoefficientOfPfe" },
                RequiredIntermediateResultFields = new[] { "R0", "θ0", "Pcon", "P0cu1", "Pfw", "Pfe", "CoefficientOfPfe" },
                RequiredStructuredPayloadSignals = new[] { "DataList.U0", "DataList.I0", "DataList.P0", "DataList.P0cu1", "DataList.Pcon", "DataList.Pfe", "DataList.n0", "DataList.T0" },
                RequiredStructuredResultSignals = new[] { "P0", "I0", "ΔI0", "Pcu", "Pfw", "Pfe", "CoefficientOfPfe" },
                MinimumRawSampleCount = 0,
                MinimumStructuredPayloadSampleCount = 3,
                MinimumStructuredResultSampleCount = 1,
                FormulaSignals = new[]
                {
                    "按 R0/R1c/K1/θ1c 反推 θ0，或反向由 θ0 推算 R0",
                    "逐点计算 U0/Un、P0cu1、Pcon，再对低压区 Pcon-U² 做线性拟合求 Pfw",
                    "对各点 Pfe 做多项式拟合，形成 CoefficientOfPfe 供堵转/负载算法复用"
                },
                LegacyAlgorithmRules = new[]
                {
                    "当 RConverseType=1 时优先由 R0 反算 θ0，否则取热态温度最大值再回算 R0",
                    "风摩损耗仅取 U0/Un<0.51 的样本做线性拟合",
                    "额定点 I0/ΔI0/P0/Pcu/Pfe 均由多项式在 1.0 pu 电压处回归得到"
                },
                LegacyDecisionAnchors = new[]
                {
                    "RConverseType=1 时走 R0→θ0 分支，否则走 θ0→R0 分支",
                    "仅 U0/Un<0.51 的样本参与 Pfw 线性拟合，低压段样本覆盖度直接影响旧算法可信度",
                    "最终 I0/ΔI0/P0/Pcu/Pfe 均取 1.0 pu 回归值，而不是简单取原始点"
                },
                Notes = "旧 FrmMotor_Y_NoLoad 会先读取直流电阻结果补 R1c/θ1c，再拟合空载曲线得到 Pfe/Pfw/CoefficientOfPfe。"
            },
            [MotorYTestMethodCodes.HeatRun] = new()
            {
                CanonicalCode = MotorYTestMethodCodes.HeatRun,
                AlgorithmEntry = MotorYLegacyAlgorithmEntrypoints.Thermal,
                RequiresRatedParams = true,
                UpstreamCanonicalCodes = new[] { MotorYTestMethodCodes.DcResistance },
                UpstreamLegacyAliases = BuildUpstreamLegacyAliases(MotorYTestMethodCodes.DcResistance),
                RequiredPayloadFields = new[] { "Data1List", "Data2List", "Rc", "θc", "Pn", "K1", "Order", "HotStateType" },
                RequiredRatedParamFields = new[] { "GB" },
                RequiredResultFields = new[] { "Rw", "Rn", "Δθ", "Δθn", "θw", "θs", "θb" },
                RequiredIntermediateResultFields = new[] { "firstSecondsInterval", "Rw", "Rn", "Rws", "θw", "θs", "θb" },
                RequiredStructuredPayloadSignals = new[] { "Data1List.Time", "Data1List.P1", "Data1List.θ1", "Data1List.AmbientTemperature", "Data2List.Time", "Data2List.R" },
                RequiredStructuredResultSignals = new[] { "Rw", "Rn", "Δθ", "Δθn", "θw", "θs", "θb" },
                MinimumRawSampleCount = 0,
                MinimumStructuredPayloadSampleCount = 2,
                MinimumStructuredResultSampleCount = 1,
                FormulaSignals = new[]
                {
                    "按额定功率 Pn 决定第一测量时间间隔 firstSecondsInterval（30/90/120s）",
                    "电阻-时间曲线拟合得到 Rw/Rn，再结合 Rc/θc 推导 Δθ、Δθn、θw、θs、Rws",
                    "θb 取试验末段 1/4 时间窗内环境温度均值"
                },
                LegacyAlgorithmRules = new[]
                {
                    "HotStateType=0 时优先使用首个实测热点电阻点，不满足间隔则外推至 firstSecondsInterval",
                    "GB1032_2012/TB_朝阳电机 与 GB1032_2023 的温升公式不同，2023 版直接使用 Rn/Rc 比值口径",
                    "θs 在热电偶/外推模式下采用不同来源：实测 θ1 或 θw+25-θb"
                },
                LegacyDecisionAnchors = new[]
                {
                    "firstSecondsInterval 由 Pn 分段决定：≤50kW=30s、≤200kW=90s、>200kW=120s",
                    "HotStateType=0 优先首个实测 Rn，HotStateType=1 强制按 firstSecondsInterval 外推",
                    "GB2012/TB 与 GB2023 温升公式不同，适配层必须保留标准分支"
                },
                Notes = "旧 FrmMotor_Y_Thermal 先读取直流电阻结果补 Rc/θc；Algorithm_Motor_Y.Thermal 再按额定功率与 GB 版本计算 Rn/Rw/Δθ/θw。"
            },
            [MotorYTestMethodCodes.LoadA] = new()
            {
                CanonicalCode = MotorYTestMethodCodes.LoadA,
                AlgorithmEntry = MotorYLegacyAlgorithmEntrypoints.LoadA,
                RequiresRatedParams = false,
                UpstreamCanonicalCodes = new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun },
                UpstreamLegacyAliases = BuildUpstreamLegacyAliases(MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun),
                RequiredPayloadFields = new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θa", "PolePairs", "Pn", "Un", "ΔT" },
                RequiredRatedParamFields = Array.Empty<string>(),
                RequiredResultFields = new[] { "Pcu1", "Pcu2", "ResultDataList", "η" },
                RequiredIntermediateResultFields = new[] { "R1t", "Pcu1t", "Nst", "St", "Ub", "Pfe", "Pcu2t", "Tx", "P2tx", "P2x", "η" },
                RequiredStructuredPayloadSignals = new[] { "RawDataList.U", "RawDataList.I1", "RawDataList.P1t", "RawDataList.Nt", "RawDataList.Tt", "RawDataList.Frequency", "RawDataList.θ1t", "ResultDataList.P2", "ResultDataList.η" },
                RequiredStructuredResultSignals = new[] { "Pcu1", "Pcu2", "η", "ResultDataList" },
                MinimumRawSampleCount = 3,
                MinimumStructuredPayloadSampleCount = 3,
                MinimumStructuredResultSampleCount = 5,
                FormulaSignals = new[]
                {
                    "逐点计算 R1t/Pcu1t/Nst/St/Ub/Pfe/Pcu2t/Tx/P2tx，再折算到 θa 条件下得到 Pcu1x/Pcu2x/Sx/Nx/P2x/η",
                    "按 P2x 对 P1t/Sx/Cosφ/I1/η 做多项式拟合，生成 ResultDataList",
                    "A法结果表固定回归 125%、100%、75%、50%、25% 五个额定负载点"
                },
                LegacyAlgorithmRules = new[]
                {
                    "铁耗一律通过空载试验给出的 CoefficientOfPfe 在 Ub/Un 上回归求得",
                    "温度修正显式依赖 θa，而不是像 B 法那样再分 GB 版本处理 θs",
                    "该算法不直接依赖额定参数对象，但要求 payload 内已有 Pn/Un/PolePairs 等额定量"
                },
                LegacyDecisionAnchors = new[]
                {
                    "必须先拿到 NoLoad 的 CoefficientOfPfe/Pfw 与 HeatRun 的 θa 口径再进入 A 法计算",
                    "A 法结果固定回归到 125/100/75/50/25% 五个额定负载点",
                    "该分支不依赖 ratedParams 对象，但 payload 内额定量缺失时仍无法对齐旧算法"
                },
                Notes = "旧 FrmMotor_Y_Load_A 会先校验空载试验的铁耗系数，且从热试验引用 θa；转矩修正还依赖耦接空载/单空载结果。"
            },
            [MotorYTestMethodCodes.LoadB] = new()
            {
                CanonicalCode = MotorYTestMethodCodes.LoadB,
                AlgorithmEntry = MotorYLegacyAlgorithmEntrypoints.LoadB,
                RequiresRatedParams = true,
                UpstreamCanonicalCodes = new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun },
                UpstreamLegacyAliases = BuildUpstreamLegacyAliases(MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun),
                RequiredPayloadFields = new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θw", "θb", "PolePairs", "Pn", "Un", "ΔT", "K1", "K2" },
                RequiredRatedParamFields = new[] { "GB" },
                RequiredResultFields = new[] { "A", "B", "R", "Pcu1", "Pcu2", "θs", "ResultDataList" },
                RequiredIntermediateResultFields = new[] { "R1t", "Pcu1t", "Nst", "St", "Ub", "Pfe", "Pcu2t", "Tx", "P2tx", "Pl", "A", "B", "R", "Ps", "cuC", "θs" },
                RequiredStructuredPayloadSignals = new[] { "RawDataList.U", "RawDataList.I1", "RawDataList.P1t", "RawDataList.Nt", "RawDataList.Tt", "RawDataList.Frequency", "RawDataList.θ1t", "RawDataList.θa", "RawDataList.Pl", "ResultDataList.P2", "ResultDataList.Ps" },
                RequiredStructuredResultSignals = new[] { "A", "B", "R", "Pcu1", "Pcu2", "θs", "ResultDataList" },
                MinimumRawSampleCount = 3,
                MinimumStructuredPayloadSampleCount = 3,
                MinimumStructuredResultSampleCount = 6,
                FormulaSignals = new[]
                {
                    "先逐点计算 R1t/Pcu1t/Nst/St/Ub/Pfe/Pcu2t/Tx/P2tx/Pl，再用 Tx²-Pl 相关关系求附加损耗系数 A/B/R",
                    "当 R<0.95 时执行一次删除坏点，再重新拟合 A/B/R",
                    "依据 GB 版本切换 θs 与 ratios 口径，并生成 ResultDataList"
                },
                LegacyAlgorithmRules = new[]
                {
                    "GB1032_2012/TB_朝阳电机 使用 1.5/1.25/1/0.75/0.5/0.25 负载点，GB1032_2023 使用 1.25/1.15/1/0.75/0.5/0.25",
                    "2012/2023 国标分支以 θw+25-θb 推导 θs，朝阳电机分支按每个负载点 θ1t/θa 单点计算 θs",
                    "结果区会循环下调铜耗系数 cuC，直到所有负载点附加损耗 Ps 非负"
                },
                LegacyDecisionAnchors = new[]
                {
                    "GB 版本决定 ratios 负载点集与 θs 计算分支，B 法不能脱离 ratedParams.GB 运行",
                    "当相关系数 R<0.95 时需先删坏点再重新拟合 A/B/R",
                    "结果区会从 cuC=1 开始逐步下调，直到所有 Ps 非负，说明旧算法存在迭代收敛决策"
                },
                Notes = "旧 FrmMotor_Y_Load_B 同时依赖空载试验的 Pfe/Pfw/R1c/θ1c 与热试验的 θw/θb；算法还按 GB 版本切换 ratios/θs 口径。"
            },
            [MotorYTestMethodCodes.LockedRotor] = new()
            {
                CanonicalCode = MotorYTestMethodCodes.LockedRotor,
                AlgorithmEntry = MotorYLegacyAlgorithmEntrypoints.LockRotor,
                RequiresRatedParams = false,
                UpstreamCanonicalCodes = new[] { MotorYTestMethodCodes.NoLoad },
                UpstreamLegacyAliases = BuildUpstreamLegacyAliases(MotorYTestMethodCodes.NoLoad),
                RequiredPayloadFields = new[] { "DataList", "CoefficientOfPfe", "Un", "In", "Tn", "PolePairs", "R1c", "θ1c", "K1", "C1" },
                RequiredRatedParamFields = Array.Empty<string>(),
                RequiredResultFields = new[] { "Ikn", "Pkn", "Tkn", "IknDivideIn", "TknDivideTn" },
                RequiredIntermediateResultFields = new[] { "θ1s", "R", "Pkcu1", "Pfe", "ns", "Tk", "Ikn", "Pkn", "Tkn" },
                RequiredStructuredPayloadSignals = new[] { "DataList.Uk", "DataList.Ik", "DataList.Pk", "DataList.Tk", "DataList.Pkcu1", "DataList.Pfe", "DataList.ns" },
                RequiredStructuredResultSignals = new[] { "Ikn", "Pkn", "Tkn", "IknDivideIn", "TknDivideTn" },
                MinimumRawSampleCount = 0,
                MinimumStructuredPayloadSampleCount = 3,
                MinimumStructuredResultSampleCount = 1,
                FormulaSignals = new[]
                {
                    "逐点计算 θ1s/R/Pkcu1/Pfe/ns/Tk，再外推/回归得到 Ikn/Pkn/Tkn",
                    "当最大试验电压 <0.9Un 时走 log-log 拟合并用最大电流点做等比换算",
                    "当试验电压落在 0.9-1.1Un 时直接对 Uk-I/P/T 曲线做多项式拟合"
                },
                LegacyAlgorithmRules = new[]
                {
                    "TorqueCalType=1 时才会先补算温升电阻与转矩相关中间量",
                    "RCalType 决定是由 R1c/θ1c/K1 动态换算电阻，还是直接使用 R1s",
                    "最终除返回 Ikn/Pkn/Tkn 外，还会同步输出 Ikn/In 与 Tkn/Tn 比值"
                },
                LegacyDecisionAnchors = new[]
                {
                    "最大堵转电压 <0.9Un 时走 log-log 拟合分支，否则走 Uk-I/P/T 多项式拟合分支",
                    "TorqueCalType=1 时才会计算 θ1s/R/Pkcu1/Pfe/ns/Tk 等中间量",
                    "RCalType 决定是冷态电阻换算还是直接使用 R1s，属于旧算法关键分支"
                },
                Notes = "旧 FrmMotor_Y_Lock_Rotor 会从空载试验引用 R1c/θ1c/K1；堵转算法再结合铁耗系数与额定量推导 Ikn/Pkn/Tkn。"
            }
        };

    public static MotorYLegacyAlgorithmDependencyProfile? TryGet(string? canonicalCode)
    {
        if (string.IsNullOrWhiteSpace(canonicalCode))
        {
            return null;
        }

        return Profiles.TryGetValue(canonicalCode, out var profile)
            ? profile
            : null;
    }
}
