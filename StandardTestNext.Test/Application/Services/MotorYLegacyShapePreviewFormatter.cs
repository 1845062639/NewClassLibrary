namespace StandardTestNext.Test.Application.Services;

public static class MotorYLegacyShapePreviewFormatter
{
    public static string FormatLockRotor(MotorYLockRotorLegacyShape? shape)
    {
        return shape is null
            ? "<invalid-lock-rotor-payload>"
            : $"Un={shape.Un}, Ikn={shape.Ikn}, Pkn={shape.Pkn}, Tkn={shape.Tkn}, points={shape.DataList.Count}";
    }

    public static string FormatThermal(MotorYThermalLegacyShape? shape)
    {
        return shape is null
            ? "<invalid-thermal-payload>"
            : $"Rc={shape.Rc}, θw={shape.θw}, Δθ={shape.Δθ}, data1={shape.Data1List.Count}, data2={shape.Data2List.Count}";
    }

    public static string FormatLoadA(MotorYLoadALegacyShape? shape)
    {
        return shape is null
            ? "<invalid-load-a-payload>"
            : $"Un={shape.Un}, Pn={shape.Pn}, raw={shape.RawDataList.Count}, result={shape.ResultDataList.Count}";
    }

    public static string FormatLoadB(MotorYLoadBLegacyShape? shape)
    {
        return shape is null
            ? "<invalid-load-b-payload>"
            : $"Un={shape.Un}, Pn={shape.Pn}, raw={shape.RawDataList.Count}, result={shape.ResultDataList.Count}, θw={shape.θw}";
    }
}
