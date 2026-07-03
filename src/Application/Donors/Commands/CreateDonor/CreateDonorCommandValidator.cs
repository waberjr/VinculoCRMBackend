using VinculoBackend.Application.Common.Interfaces;

namespace VinculoBackend.Application.Donors.Commands.CreateDonor;

public sealed class CreateDonorCommandValidator : AbstractValidator<CreateDonorCommand>
{
    public CreateDonorCommandValidator(IBrazilianDocumentValidator documentValidator)
    {
        RuleFor(v => v.FullName).NotEmpty().MaximumLength(200);
        RuleFor(v => v.PersonType).NotEmpty().MaximumLength(80);
        RuleFor(v => v.Status).NotEmpty().MaximumLength(80);
        RuleFor(v => v.Source).MaximumLength(80);
        RuleFor(v => v.RelationshipProfile).MaximumLength(80);
        RuleFor(v => v.PreferredContactChannel).MaximumLength(80);
        RuleFor(v => v.Document)
            .MaximumLength(32)
            .Must((command, document) => BeValidDocument(command.PersonType, document, documentValidator))
            .When(v => !string.IsNullOrWhiteSpace(v.Document))
            .WithMessage("CPF/CNPJ inválido para o tipo de pessoa informado.");
        RuleFor(v => v.Email).MaximumLength(254).EmailAddress().When(v => !string.IsNullOrWhiteSpace(v.Email));
        RuleFor(v => v.Phone).MaximumLength(32);
        RuleFor(v => v.WhatsApp).MaximumLength(32);
        RuleForEach(v => v.Phones).ChildRules(phone =>
        {
            phone.RuleFor(v => v.TypeCode).NotEmpty().MaximumLength(80);
            phone.RuleFor(v => v.Number).NotEmpty().MaximumLength(32);
        });
        RuleForEach(v => v.Emails).ChildRules(email =>
        {
            email.RuleFor(v => v.TypeCode).NotEmpty().MaximumLength(80);
            email.RuleFor(v => v.Address).NotEmpty().MaximumLength(254).EmailAddress();
        });
        RuleFor(v => v.City).MaximumLength(120);
        RuleFor(v => v.State).MaximumLength(2);
        RuleFor(v => v.AddressLine1).MaximumLength(250);
        RuleFor(v => v.AddressLine2).MaximumLength(250);
        RuleFor(v => v.PostalCode).MaximumLength(16);
        RuleFor(v => v.Notes).MaximumLength(1000);
        RuleFor(v => v.DoNotContactReason).NotEmpty().When(v => v.DoNotContact);
    }

    private static bool BeValidDocument(string personType, string? document, IBrazilianDocumentValidator documentValidator)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return true;
        }

        return string.Equals(personType, "Company", StringComparison.OrdinalIgnoreCase)
            ? documentValidator.IsValidCnpj(document)
            : documentValidator.IsValidCpf(document);
    }
}
