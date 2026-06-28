namespace VinculoBackend.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string name, string key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}
