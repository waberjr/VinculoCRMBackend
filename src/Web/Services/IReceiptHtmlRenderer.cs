using VinculoBackend.Application.Receipts.Models;

namespace VinculoBackend.Web.Services;

public interface IReceiptHtmlRenderer
{
    string Render(ReceiptPrintDto receipt);
}
