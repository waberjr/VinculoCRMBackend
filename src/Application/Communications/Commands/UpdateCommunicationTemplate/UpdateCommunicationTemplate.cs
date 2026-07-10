using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Communications.Services;

namespace VinculoBackend.Application.Communications.Commands.UpdateCommunicationTemplate;

public sealed record UpdateCommunicationTemplateCommand(
    Guid Id,
    string Name,
    string Channel,
    string? Subject,
    string Body,
    IReadOnlyCollection<string> Variables,
    bool IsActive) : IRequest;

public sealed class UpdateCommunicationTemplateCommandHandler : IRequestHandler<UpdateCommunicationTemplateCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCommunicationTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCommunicationTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.CommunicationTemplates.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (template is null)
        {
            throw new global::VinculoBackend.Application.Common.Exceptions.NotFoundException(nameof(template), request.Id.ToString());
        }

        var channel = CommunicationChannelParser.Parse(request.Channel, nameof(request.Channel));
        template.Update(
            request.Name,
            channel,
            request.Subject,
            request.Body,
            request.Variables,
            request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
