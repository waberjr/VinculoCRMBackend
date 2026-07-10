using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.DonorTags.Commands.CreateDonorTag;

public record CreateDonorTagCommand(string Name, string? Description) : IRequest<Guid>;

public sealed class CreateDonorTagCommandHandler : IRequestHandler<CreateDonorTagCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public CreateDonorTagCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<Guid> Handle(CreateDonorTagCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var tag = DonorTag.Create(organizationId, request.Name, request.Description);

        _context.DonorTags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);

        return tag.Id;
    }
}
