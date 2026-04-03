namespace StandardTestNext.Test.Application.Services;

internal static class MotorYMethodRouteSelectionSnapshotFactory
{
    public static MotorYMethodRouteSelectionSnapshot Create(MotorYMethodDecisionSnapshot snapshot)
    {
        return MotorYMethodDecisionFactory.CreateSelection(snapshot);
    }
}
