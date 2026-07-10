using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Common.Models;
using VinculoBackend.Application.Communications.Services;
using VinculoBackend.Domain.Entities;

namespace VinculoBackend.Application.Communications.Commands.CreateCommunicationTemplate;

public sealed record CreateCommunicationTemplateCommand(
    string Name,
    string Channel,
    string? Subject,
    string Body,
    IReadOnlyCollection<string> Variables) : IRequest<Guid>;

public sealed class CreateCommunicationTemplateCommandHandler : IRequestHandler<CreateCommunicationTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IOrganizationContext _organizationContext;

    public CreateCommunicationTemplateCommandHandler(IApplicationDbContext context, IOrganizationContext organizationContext)
    {
        _context = context;
        _organizationContext = organizationContext;
    }

    public async Task<Guid> Handle(CreateCommunicationTemplateCommand request, CancellationToken cancellationToken)
    {
        var organizationId = RequiredOrganization.From(_organizationContext);
        var channel = CommunicationChannelParser.Parse(request.Channel, nameof(request.Channel));
        var template = CommunicationTemplate.Create(
            organizationId,
            request.Name,
            channel,
            request.Subject,
            request.Body,
            request.Variables);

        _context.CommunicationTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template.Id;
    }
}
