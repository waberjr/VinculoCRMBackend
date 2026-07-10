using VinculoBackend.Application.Receipts.Models;

namespace VinculoBackend.Application.Common.Interfaces;

public sealed record ReceiptPdfLogo(string ContentType, byte[] Content);

public interface IReceiptPdfGenerator
{
    byte[] Generate(ReceiptPrintDto receipt, ReceiptPdfLogo? logo);
}
