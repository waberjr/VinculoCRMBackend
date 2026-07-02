namespace VinculoBackend.Application.Common.Interfaces;

public interface IBrazilianDocumentValidator
{
    string Normalize(string document);

    bool IsValidCpf(string document);

    bool IsValidCnpj(string document);
}
