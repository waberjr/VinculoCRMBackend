using System.Runtime.CompilerServices;
using NUnit.Framework;
using Shouldly;

namespace VinculoBackend.Application.UnitTests.Common.Architecture;

public class EndpointArchitectureTests
{
    private static readonly string[] ForbiddenTokens =
    [
        "IApplicationDbContext",
        "SaveChangesAsync",
        ".AsNoTracking()",
        ".Add(",
        ".Remove(",
        ".FirstOrDefaultAsync(",
        ".ToListAsync(",
        ".AnyAsync(",
        "RequiredOrganization.From",
        "IFileStorageService",
    ];

    [Test]
    public void EndpointsShouldNotContainPersistenceOrBusinessUseCaseLogic()
    {
        var endpointsPath = GetEndpointsPath();
        var endpointFiles = Directory.GetFiles(endpointsPath, "*.cs", SearchOption.AllDirectories);

        var violations = endpointFiles
            .SelectMany(file => ForbiddenTokens
                .Where(token => File.ReadAllText(file).Contains(token, StringComparison.Ordinal))
                .Select(token => $"{Path.GetFileName(file)} contains {token}"))
            .ToArray();

        violations.ShouldBeEmpty();
    }

    private static string GetEndpointsPath([CallerFilePath] string currentFile = "")
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(currentFile)!, "..", "..", "..", ".."));
        return Path.Combine(repositoryRoot, "src", "Web", "Endpoints");
    }
}
