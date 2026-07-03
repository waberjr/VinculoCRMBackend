using Aspire.Hosting;
using VinculoBackend.Shared;

namespace VinculoBackend.TestAppHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder.AddMySql(Services.DatabaseServer)
            .AddDatabase(Services.Database);

        builder.Build().Run();
    }
}
