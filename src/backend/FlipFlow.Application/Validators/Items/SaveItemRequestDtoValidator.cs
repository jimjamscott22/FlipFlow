using FlipFlow.Application.Contracts.Items;
using FluentValidation;

namespace FlipFlow.Application.Validators.Items;

public sealed class SaveItemRequestDtoValidator : AbstractValidator<SaveItemRequestDto>
{
    public SaveItemRequestDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Brand)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Model)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Category)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.AskingPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Condition)
            .IsInEnum();

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
