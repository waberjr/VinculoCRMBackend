using FluentValidation.Results;
using VinculoBackend.Domain.Enums;

namespace VinculoBackend.Application.Communications.Services;

public static class CommunicationChannelParser
{
    public static CommunicationChannel Parse(string value, string propertyName)
    {
        if (Enum.TryParse<CommunicationChannel>(value, ignoreCase: true, out var channel))
        {
            return channel;
        }

        throw new global::VinculoBackend.Application.Common.Exceptions.ValidationException([
            new ValidationFailure(propertyName, "Canal de comunicacao invalido.")
        ]);
    }
}
