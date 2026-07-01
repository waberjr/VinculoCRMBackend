namespace VinculoBackend.Application.Users.Models;

public sealed record CurrentOrganizationDto(Guid Id, string Name, decimal? DefaultMonthlyGoal, string Role);

public sealed record CurrentUserDto(
    string Id,
    string DisplayName,
    string Email,
    string Role,
    bool IsSystemAdministrator,
    CurrentOrganizationDto Organization,
    IReadOnlyCollection<CurrentOrganizationDto> Organizations);

public sealed record AttendantDto(string Id, string DisplayName, string Email);
