namespace VinculoBackend.Application.Locations.Models;

public sealed record StateDto(int Id, string Code, string Name);

public sealed record CityDto(int Id, string Name);
