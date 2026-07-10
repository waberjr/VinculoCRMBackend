using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Receipts.Models;
using VinculoBackend.Application.Receipts.Queries.GetReceiptPrint;

namespace VinculoBackend.Application.Receipts.Queries.GetReceiptPdf;

public sealed record GetReceiptPdfQuery(Guid Id) : IRequest<ReceiptPdfDto?>;

public sealed class GetReceiptPdfQueryHandler : IRequestHandler<GetReceiptPdfQuery, ReceiptPdfDto?>
{
    private readonly ISender _sender;
    private readonly IFileStorageService _fileStorageService;
    private readonly IReceiptPdfGenerator _receiptPdfGenerator;

    public GetReceiptPdfQueryHandler(
        ISender sender,
        IFileStorageService fileStorageService,
        IReceiptPdfGenerator receiptPdfGenerator)
    {
        _sender = sender;
        _fileStorageService = fileStorageService;
        _receiptPdfGenerator = receiptPdfGenerator;
    }

    public async Task<ReceiptPdfDto?> Handle(GetReceiptPdfQuery request, CancellationToken cancellationToken)
    {
        var receipt = await _sender.Send(new GetReceiptPrintQuery(request.Id), cancellationToken);
        if (receipt is null)
        {
            return null;
        }

        var logo = await LoadLogoAsync(receipt, cancellationToken);
        return new ReceiptPdfDto($"{receipt.Number}.pdf", _receiptPdfGenerator.Generate(receipt, logo));
    }

    private async Task<ReceiptPdfLogo?> LoadLogoAsync(ReceiptPrintDto receipt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(receipt.OrganizationLogoUrl) ||
            !receipt.OrganizationLogoUrl.StartsWith("storage://", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var download = await _fileStorageService.OpenReadAsync(receipt.OrganizationLogoUrl, cancellationToken);
        if (download is null)
        {
            return null;
        }

        await using var stream = download.Content;
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        return new ReceiptPdfLogo(download.ContentType, memory.ToArray());
    }
}
