namespace StandardTestNext.Test.Application.Services;

public static class TestReportSelection
{
    public static TestReportSnapshot? SelectPrimary(IReadOnlyList<TestReportSnapshot> reports)
    {
        return reports.FirstOrDefault(x => x.IsPrimaryEntry)
               ?? reports.FirstOrDefault(x => string.Equals(x.Format, "json", StringComparison.OrdinalIgnoreCase))
               ?? reports.OrderByDescending(x => x.SavedAt).FirstOrDefault();
    }

    public static TestReportSnapshot? SelectLightweight(IReadOnlyList<TestReportSnapshot> reports)
    {
        return reports.FirstOrDefault(x => x.IsLightweightEntry)
               ?? reports.FirstOrDefault(x => string.Equals(x.Format, "manifest", StringComparison.OrdinalIgnoreCase))
               ?? reports.OrderBy(x => x.SavedAt).FirstOrDefault();
    }
}
