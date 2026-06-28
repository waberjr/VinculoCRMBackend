namespace VinculoBackend.Application.Donors.Commands.CreateDonor;

public sealed class CreateDonorCommandValidator : AbstractValidator<CreateDonorCommand>
{
    public CreateDonorCommandValidator()
    {
        RuleFor(v => v.FullName).NotEmpty().MaximumLength(200);
        RuleFor(v => v.PersonTypeOptionId).NotEmpty();
        RuleFor(v => v.StatusOptionId).NotEmpty();
        RuleFor(v => v.Document).MaximumLength(32);
        RuleFor(v => v.Email).MaximumLength(254).EmailAddress().When(v => !string.IsNullOrWhiteSpace(v.Email));
        RuleFor(v => v.Phone).MaximumLength(32);
        RuleFor(v => v.WhatsApp).MaximumLength(32);
        RuleFor(v => v.City).MaximumLength(120);
        RuleFor(v => v.State).MaximumLength(2);
        RuleFor(v => v.AddressLine1).MaximumLength(250);
        RuleFor(v => v.AddressLine2).MaximumLength(250);
        RuleFor(v => v.PostalCode).MaximumLength(16);
        RuleFor(v => v.Notes).MaximumLength(1000);
    }
}
