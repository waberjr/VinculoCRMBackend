using System.Text;
using DocumentValidator;
using VinculoBackend.Application.Common.Interfaces;

namespace VinculoBackend.Infrastructure.Documents;

public sealed class DocsBrBrazilianDocumentValidator : IBrazilianDocumentValidator
{
    public string Normalize(string document)
    {
        var builder = new StringBuilder(document.Length);
        foreach (var character in document)
        {
            if (char.IsDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    public bool IsValidCpf(string document) => CpfValidation.Validate(document);

    public bool IsValidCnpj(string document) => CnpjValidation.Validate(document);
}
