using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Domain.Entities;

public class CommunicationTemplate : OrganizationEntity
{
    public string Name { get; set; } = string.Empty;
    public CommunicationChannel Channel { get; set; } = CommunicationChannel.Email;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public string Variables { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public static CommunicationTemplate Create(
        Guid organizationId,
        string name,
        CommunicationChannel channel,
        string? subject,
        string body,
        IEnumerable<string> variables)
    {
        var template = new CommunicationTemplate { OrganizationId = organizationId };
        template.Update(name, channel, subject, body, variables, true);
        return template;
    }

    public void Update(
        string name,
        CommunicationChannel channel,
        string? subject,
        string body,
        IEnumerable<string> variables,
        bool isActive)
    {
        Name = name.Trim();
        Channel = channel;
        Subject = subject?.Trim();
        Body = body.Trim();
        Variables = string.Join(",", variables.Select(variable => variable.Trim()).Where(variable => variable.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase));
        IsActive = isActive;
    }
}
