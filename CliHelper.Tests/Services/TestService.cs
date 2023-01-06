namespace CliHelper.Tests.Services;

public class TestService : ITestService
{
    public const string Response = "eiohtoihtseoitrjes";

    public string GetResponse() => Response;
}