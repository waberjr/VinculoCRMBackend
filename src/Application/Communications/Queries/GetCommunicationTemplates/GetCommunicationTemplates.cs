using VinculoBackend.Application.Common.Interfaces;
using VinculoBackend.Application.Communications.Models;

namespace VinculoBackend.Application.Communications.Queries.GetCommunicationTemplates;

public sealed record GetCommunicationTemplatesQuery : IRequest<IReadOnlyCollection<CommunicationTemplateDto>>;

public sealed class GetCommunicationTemplatesQueryHandler : IRequestHandler<GetCommunicationTemplatesQuery, IReadOnlyCollection<CommunicationTemplateDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCommunicationTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<CommunicationTemplateDto>> Handle(GetCommunicationTemplatesQuery request, CancellationToken cancellationToken)
    {
        return await _context.CommunicationTemplates
            .AsNoTracking()
            .OrderBy(template => template.Name)
            .Select(template => new CommunicationTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Channel = template.Channel.ToString(),
                Subject = template.Subject,
                Body = template.Body,
                Variables = template.Variables.Length == 0
                    ? Array.Empty<string>()
                    : template.Variables.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                IsActive = template.IsActive,
                Created = template.Created,
            })
            .ToListAsync(cancellationToken);
    }
}
