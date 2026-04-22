using FluentValidation;
using PharmacyApp.Application.Contracts.Review;

namespace PharmacyApp.Application.DTOValidations.Review;

public class CreateProductReviewDtoValidator : AbstractValidator<CreateProductReviewDto>
{
    public CreateProductReviewDtoValidator()
    {
        RuleFor(dto => dto.ProductId)
            .GreaterThan(0)
            .WithMessage("ProductId must be greater than 0.");

        RuleFor(dto => dto.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(dto => dto.Content)
            .NotEmpty()
            .WithMessage("Review content is required.")
            .MaximumLength(1000)
            .WithMessage("Review content must not exceed 1000 characters.");
    }
}
