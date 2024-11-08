using Fintrellis.Services.Models;
using FluentValidation;

namespace Fintrellis.Services.Validators
{
    public class PostCreateRequestValidator : AbstractValidator<PostCreateRequest>
    {
        public PostCreateRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
            RuleFor(x => x.Content).NotEmpty().WithMessage("Content is required.");
            RuleFor(x => x.PublishedDate).LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Published date cannot be in the future.");
            RuleFor(x => x.Author).NotEmpty().WithMessage("Author is required.");
        }
    }
}
