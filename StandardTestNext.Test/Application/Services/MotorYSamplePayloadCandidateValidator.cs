namespace StandardTestNext.Test.Application.Services;

internal static class MotorYSamplePayloadCandidateValidator
{
    public static bool IsValidBaselineCandidate(string canonicalCode, string dataJson)
    {
        switch (canonicalCode)
        {
            case var code when code == MotorYTestMethodCodes.DcResistance:
                return dataJson.Contains("\"Ruv\"", StringComparison.Ordinal);

            case var code when code == MotorYTestMethodCodes.NoLoad:
                var noLoad = MotorYNoLoadLegacyShape.FromJson(dataJson);
                return noLoad is not null
                    && ((noLoad.DataList.Count > 0 && (noLoad.P0 > 0 || noLoad.I0 > 0 || noLoad.DataList.Any(x => x.P0 > 0 || x.I0 > 0)))
                        || (noLoad.HasSinglePointSnapshot && (noLoad.P0 > 0 || noLoad.I0 > 0 || noLoad.ExtraFields.ContainsKey("U0"))));

            case var code when code == MotorYTestMethodCodes.HeatRun:
                var heatRun = MotorYThermalLegacyShape.FromJson(dataJson);
                return heatRun is not null
                    && heatRun.Data1List.Count > 0
                    && heatRun.Data2List.Count > 0
                    && (heatRun.DeltaThetaObserved > 0 || heatRun.θw > 0 || heatRun.Rw > 0 || heatRun.Data1List.Any(x => x.θ1 > 0 || x.P1 > 0) || heatRun.Data2List.Any(x => x.R > 0));

            case var code when code == MotorYTestMethodCodes.LoadA:
                var loadA = MotorYLoadALegacyShape.FromJson(dataJson);
                return loadA is not null
                    && loadA.RawDataList.Count > 0
                    && loadA.ResultDataList.Count > 0
                    && (loadA.Un > 0 || loadA.Pn > 0 || loadA.HasTorqueModifyFlag || loadA.RawDataList.Any(x => x.U > 0 || x.I1 > 0 || x.P1t > 0));

            case var code when code == MotorYTestMethodCodes.LoadB:
                var loadB = MotorYLoadBLegacyShape.FromJson(dataJson);
                return loadB is not null
                    && loadB.RawDataList.Count > 0
                    && loadB.ResultDataList.Count > 0
                    && (loadB.Pfw > 0 || loadB.Pcu1 > 0 || loadB.Pcu2 > 0 || loadB.RawDataList.Any(x => x.U > 0 || x.I1 > 0 || x.P1t > 0));

            case var code when code == MotorYTestMethodCodes.LockedRotor:
                var lockedRotor = MotorYLockRotorLegacyShape.FromJson(dataJson);
                return lockedRotor is not null
                    && lockedRotor.DataList.Count > 0
                    && (lockedRotor.Ikn > 0 || lockedRotor.Pkn > 0 || lockedRotor.DataList.Any(x => x.Uk > 0 || x.Ik > 0 || x.Pk > 0 || x.Tk > 0));

            default:
                return false;
        }
    }
}
