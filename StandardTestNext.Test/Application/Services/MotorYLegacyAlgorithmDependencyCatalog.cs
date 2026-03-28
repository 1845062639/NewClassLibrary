namespace StandardTestNext.Test.Application.Services;

/// <summary>
/// 结构化沉淀旧 Motor_Y 算法入口对前置试验数据、额定参数与关键字段的依赖画像，
/// 供 next-gen 适配计划 / App 提示 / 后续算法 adapter 直接消费。
/// 目标不是现在就重写算法，而是先把“要对齐旧算法至少需要什么输入”显式化并锁进闭环。
/// </summary>
public sealed class MotorYLegacyAlgorithmDependencyProfile
{
    public string CanonicalCode { get; init; } = string.Empty;
    public string AlgorithmEntry { get; init; } = string.Empty;
    public bool RequiresRatedParams { get; init; }
    public IReadOnlyList<string> UpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredPayloadFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> RequiredRatedParamFields { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingUpstreamCanonicalCodes { get; init; } = Array.Empty<string>();
    public string Notes { get; init; } = string.Empty;
}

public static class MotorYLegacyAlgorithmDependencyCatalog
{
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
                Notes = "旧 FrmMotor_Y_Direct_Current_Resistance 页面直接采集/整理后入库，供空载/热试验继续引用 R1/θ1c。"
            },
            [MotorYTestMethodCodes.NoLoad] = new()
            {
                CanonicalCode = MotorYTestMethodCodes.NoLoad,
                AlgorithmEntry = MotorYLegacyAlgorithmEntrypoints.NoLoad,
                RequiresRatedParams = false,
                UpstreamCanonicalCodes = new[] { MotorYTestMethodCodes.DcResistance },
                RequiredPayloadFields = new[] { "DataList", "Un", "R1c", "θ1c", "K1", "Order" },
                RequiredRatedParamFields = Array.Empty<string>(),
                Notes = "旧 FrmMotor_Y_NoLoad 会先读取直流电阻结果补 R1c/θ1c，再拟合空载曲线得到 Pfe/Pfw/CoefficientOfPfe。"
            },
            [MotorYTestMethodCodes.HeatRun] = new()
            {
                CanonicalCode = MotorYTestMethodCodes.HeatRun,
                AlgorithmEntry = MotorYLegacyAlgorithmEntrypoints.Thermal,
                RequiresRatedParams = true,
                UpstreamCanonicalCodes = new[] { MotorYTestMethodCodes.DcResistance },
                RequiredPayloadFields = new[] { "Data1List", "Data2List", "Rc", "θc", "Pn", "K1", "Order", "HotStateType" },
                RequiredRatedParamFields = new[] { "GB" },
                Notes = "旧 FrmMotor_Y_Thermal 先读取直流电阻结果补 Rc/θc；Algorithm_Motor_Y.Thermal 再按额定功率与 GB 版本计算 Rn/Rw/Δθ/θw。"
            },
            [MotorYTestMethodCodes.LoadA] = new()
            {
                CanonicalCode = MotorYTestMethodCodes.LoadA,
                AlgorithmEntry = MotorYLegacyAlgorithmEntrypoints.LoadA,
                RequiresRatedParams = false,
                UpstreamCanonicalCodes = new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun },
                RequiredPayloadFields = new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θa", "PolePairs", "Pn", "Un", "ΔT" },
                RequiredRatedParamFields = Array.Empty<string>(),
                Notes = "旧 FrmMotor_Y_Load_A 会先校验空载试验的铁耗系数，且从热试验引用 θa；转矩修正还依赖耦接空载/单空载结果。"
            },
            [MotorYTestMethodCodes.LoadB] = new()
            {
                CanonicalCode = MotorYTestMethodCodes.LoadB,
                AlgorithmEntry = MotorYLegacyAlgorithmEntrypoints.LoadB,
                RequiresRatedParams = true,
                UpstreamCanonicalCodes = new[] { MotorYTestMethodCodes.NoLoad, MotorYTestMethodCodes.HeatRun },
                RequiredPayloadFields = new[] { "RawDataList", "CoefficientOfPfe", "Pfw", "R1c", "θ1c", "θw", "θb", "PolePairs", "Pn", "Un", "ΔT", "K1", "K2" },
                RequiredRatedParamFields = new[] { "GB" },
                Notes = "旧 FrmMotor_Y_Load_B 同时依赖空载试验的 Pfe/Pfw/R1c/θ1c 与热试验的 θw/θb；算法还按 GB 版本切换 ratios/θs 口径。"
            },
            [MotorYTestMethodCodes.LockedRotor] = new()
            {
                CanonicalCode = MotorYTestMethodCodes.LockedRotor,
                AlgorithmEntry = MotorYLegacyAlgorithmEntrypoints.LockRotor,
                RequiresRatedParams = false,
                UpstreamCanonicalCodes = new[] { MotorYTestMethodCodes.NoLoad },
                RequiredPayloadFields = new[] { "DataList", "CoefficientOfPfe", "Un", "In", "Tn", "PolePairs", "R1c", "θ1c", "K1", "C1" },
                RequiredRatedParamFields = Array.Empty<string>(),
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
