using Fintrellis.Services.Models;
using Fintrellis.Services.Validators;
using FluentValidation.TestHelper;

namespace Fintrellis.Services.Tests.Validators
{
    public class PostUpdateRequestValidatorTests
    {
        private readonly PostUpdateRequestValidator _validator;

        public PostUpdateRequestValidatorTests()
        {
            _validator = new PostUpdateRequestValidator();
        }

        [Fact]
        public void Should_HaveError_When_TitleIsEmpty()
        {
            var model = new PostUpdateRequest { Title = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title).WithErrorMessage("Title is required.");
        }

        [Fact]
        public void Should_HaveError_When_ContentIsEmpty()
        {
            var model = new PostUpdateRequest { Content = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Content).WithErrorMessage("Content is required.");
        }

        [Fact]
        public void Should_HaveError_When_PublishedDateIsInFuture()
        {
            var model = new PostUpdateRequest { PublishedDate = DateTime.UtcNow.AddDays(1) };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.PublishedDate).WithErrorMessage("Published date cannot be in the future.");
        }

        [Fact]
        public void Should_HaveError_When_AuthorIsEmpty()
        {
            var model = new PostUpdateRequest { Author = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Author).WithErrorMessage("Author is required.");
        }

        [Fact]
        public void Should_NotHaveAnyErrors_When_AllFieldsAreValid()
        {
            var model = new PostUpdateRequest
            {
                Title = "Valid Title",
                Content = "Valid Content",
                PublishedDate = DateTime.UtcNow.AddMinutes(-1),
                Author = "Author Name"
            };

            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
