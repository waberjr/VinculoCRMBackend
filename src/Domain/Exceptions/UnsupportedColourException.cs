namespace VinculoBackend.Domain.Exceptions;

public class UnsupportedColourException : Exception
{
    public UnsupportedColourException(string code)
        : base($"A cor \"{code}\" não é suportada.")
    {
    }
}
