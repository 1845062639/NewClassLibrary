namespace StandardTestNext.Test.Application.Services;

public static class TestRecordItemDescriptorResolver
{
    public static string ResolveDisplayName(string itemCode, string? recordMode)
    {
        var canonicalItemCode = MotorYLegacyItemCodeNormalizer.Normalize(itemCode);

        return canonicalItemCode switch
        {
            "MotorY.DcResistance" => "Motor_Y DC Resistance",
            "MotorY.NoLoad" => "Motor_Y No-Load Test",
            "MotorY.HeatRun" => "Motor_Y Heat Run Test",
            "MotorY.LoadA" => "Motor_Y Load Test A",
            "MotorY.LoadB" => "Motor_Y Load Test B",
            "MotorY.LockedRotor" => "Motor_Y Locked-Rotor Test",
            "RealtimeKeyPoints" => "Realtime Key Points",
            "RealtimeContinuous" => "Realtime Continuous Samples",
            _ => !string.IsNullOrWhiteSpace(recordMode)
                ? $"{itemCode} ({recordMode})"
                : itemCode
        };
    }

    public static int ResolveSortOrder(string itemCode, string? recordMode)
    {
        var canonicalItemCode = MotorYLegacyItemCodeNormalizer.Normalize(itemCode);

        return canonicalItemCode switch
        {
            "MotorY.DcResistance" => 10,
            "MotorY.NoLoad" => 20,
            "MotorY.HeatRun" => 30,
            "MotorY.LoadA" => 40,
            "MotorY.LoadB" => 50,
            "MotorY.LockedRotor" => 60,
            "RealtimeKeyPoints" => 100,
            "RealtimeContinuous" => 200,
            _ => string.Equals(recordMode, TestRecordSampleModes.KeyPointOnly, StringComparison.OrdinalIgnoreCase)
                ? 1000
                : string.Equals(recordMode, TestRecordSampleModes.Continuous, StringComparison.OrdinalIgnoreCase)
                    ? 2000
                    : 9000
        };
    }
}
