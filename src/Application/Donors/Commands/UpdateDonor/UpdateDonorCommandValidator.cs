namespace VinculoBackend.Application.Donors.Commands.UpdateDonor;

public sealed class UpdateDonorCommandValidator : AbstractValidator<UpdateDonorCommand>
{
    public UpdateDonorCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.FullName).NotEmpty().MaximumLength(200);
        RuleFor(v => v.PersonType).NotEmpty().MaximumLength(80);
        RuleFor(v => v.Status).NotEmpty().MaximumLength(80);
        RuleFor(v => v.Email).MaximumLength(254).EmailAddress().When(v => !string.IsNullOrWhiteSpace(v.Email));
        RuleFor(v => v.Document).MaximumLength(32);
        RuleFor(v => v.Phone).MaximumLength(32);
        RuleFor(v => v.WhatsApp).MaximumLength(32);
        RuleFor(v => v.City).MaximumLength(120);
        RuleFor(v => v.State).MaximumLength(2);
        RuleFor(v => v.Notes).MaximumLength(1000);
        RuleFor(v => v.DoNotContactReason).NotEmpty().When(v => v.DoNotContact);
    }
}
