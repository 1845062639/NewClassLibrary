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

        return sb.ToString();
    }
}
