using StandardTestNext.Test.Application.Services;

namespace StandardTestNext.Test.Application;

public sealed class TestBootstrap
{
    public void Run()
    {
        Console.WriteLine("StandardTestNext.Test starting...");

        var service = new MotorTestSessionService();
        service.PrintReadyState();

        Console.WriteLine("StandardTestNext.Test ready.");
    }
}
