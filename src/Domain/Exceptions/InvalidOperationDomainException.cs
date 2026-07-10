namespace VinculoBackend.Domain.Exceptions;

public sealed class InvalidOperationDomainException : DomainException
{
    public InvalidOperationDomainException(string message)
        : base(message)
    {
    }
}
