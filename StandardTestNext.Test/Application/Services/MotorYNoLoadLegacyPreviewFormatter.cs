using System.Text;

namespace StandardTestNext.Test.Application.Services;

public static class MotorYNoLoadLegacyPreviewFormatter
{
    public static string Format(MotorYNoLoadLegacyShape? shape)
    {
        if (shape is null)
        {
            return "<invalid-no-load-payload>";
        }

        var sb = new StringBuilder();
        sb.Append($"Un={shape.Un}, I0={shape.I0}, P0={shape.P0}, Pfw={shape.Pfw}, Pfe={shape.Pfe}, points={shape.DataList.Count}");
        if (shape.DataList.Count > 0)
        {
            var first = shape.DataList[0];
            var last = shape.DataList[^1];
            sb.Append($", first(U0={first.U0}, I0={first.I0}, P0={first.P0})");
            sb.Append($", last(U0={last.U0}, I0={last.I0}, P0={last.P0})");
        }

        if (shape.U0DivideUnIsEquesToOne_I0 > 0
            || shape.U0DivideUnIsEquesToOne_P0 > 0
            || shape.U0DivideUnIsEquesToOne_Pcu > 0
            || shape.U0DivideUnIsEquesToOne_Pfe > 0
            || shape.U0DivideUnIsEquesToOne_R0 > 0
            || shape.U0DivideUnIsEquesToOne_Theta0 > 0)
        {
            sb.Append($", pu1(I0={shape.U0DivideUnIsEquesToOne_I0}, P0={shape.U0DivideUnIsEquesToOne_P0}, Pcu={shape.U0DivideUnIsEquesToOne_Pcu}, Pfe={shape.U0DivideUnIsEquesToOne_Pfe}, R0={shape.U0DivideUnIsEquesToOne_R0}, θ0={shape.U0DivideUnIsEquesToOne_Theta0})");
        }

        sb.Append($", pfw-fit(samples={shape.PfwFitSampleCount}, ready={shape.PfwFitWindowReady})");
        return sb.ToString();
    }
}
