using StandardTestNext.App.Application.Services;
using StandardTestNext.Contracts;
using StandardTestNext.Test.Application.Abstractions;
using StandardTestNext.Test.Application.AppSide;
using StandardTestNext.Test.Application.Services;
using StandardTestNext.Test.Infrastructure.Persistence;

namespace StandardTestNext.App.Application;

public static class InProcAppQueryGatewayFactory
{
    public static DefaultQueryGatewayResolution ResolveDefaultGateway(AppQueryGatewayMode mode = AppQueryGatewayMode.Auto, string? sqliteDbPath = null)
    {
        var gateway = mode switch
        {
            AppQueryGatewayMode.SeededInProc => TestRecordQueryGatewayFactory.Create(TryCreateSeededGateway),
            AppQueryGatewayMode.SqliteInProc => TestRecordQueryGatewayFactory.Create(() => TryCreateSqliteGateway(sqliteDbPath)),
            AppQueryGatewayMode.NullFallback => TestRecordQueryGatewayFactory.Create(),
            _ => TestRecordQueryGatewayFactory.Create(() => TryCreateSqliteGateway(sqliteDbPath) ?? TryCreateSeededGateway())
        };

        return new DefaultQueryGatewayResolution
        {
            Gateway = gateway,
            ResolutionKind = gateway switch
            {
                { } when TestRecordQueryGatewayFactory.IsNullGateway(gateway) => DefaultQueryGatewayResolutionKind.NullFallback,
                { } when mode == AppQueryGatewayMode.SqliteInProc || (mode == AppQueryGatewayMode.Auto && !string.IsNullOrWhiteSpace(sqliteDbPath) && !TestRecordQueryGatewayFactory.IsNullGateway(gateway)) => DefaultQueryGatewayResolutionKind.SqliteInProc,
                _ => DefaultQueryGatewayResolutionKind.SeededInProc
            }
        };
    }

    private static ITestRecordQueryGateway? TryCreateSeededGateway()
    {
        return InProcAppQueryGatewaySeedFactory.CreateSeededGateway();
    }

    private static ITestRecordQueryGateway? TryCreateSqliteGateway(string? sqliteDbPath)
    {
        var resolvedPath = ResolveSqliteDbPath(sqliteDbPath);
        if (string.IsNullOrWhiteSpace(resolvedPath) || !File.Exists(resolvedPath))
        {
            return null;
        }

        ITestRecordRepository recordRepository = new SQLiteTestRecordRepository(resolvedPath);
        IRecordAttachmentRepository attachmentRepository = new SQLiteRecordAttachmentRepository(resolvedPath);
        ITestReportRepository reportRepository = new SQLiteTestReportRepository(resolvedPath);
        var queryService = new TestRecordQueryService(recordRepository, attachmentRepository, reportRepository);
        var facade = new TestRecordQueryFacade(queryService);
        return new TestRecordQueryGatewayAdapter(facade);
    }

    private static string? ResolveSqliteDbPath(string? sqliteDbPath)
    {
        if (string.IsNullOrWhiteSpace(sqliteDbPath))
        {
            return null;
        }

        return Path.GetFullPath(sqliteDbPath);
    }
}

public sealed class DefaultQueryGatewayResolution
{
    public ITestRecordQueryGateway Gateway { get; init; } = TestRecordQueryGatewayFactory.Create();
    public DefaultQueryGatewayResolutionKind ResolutionKind { get; init; }
}

public enum DefaultQueryGatewayResolutionKind
{
    SeededInProc,
    SqliteInProc,
    NullFallback
}
