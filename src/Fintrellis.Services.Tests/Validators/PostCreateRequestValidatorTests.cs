using Fintrellis.Services.Models;
using Fintrellis.Services.Validators;
using FluentValidation.TestHelper;

namespace Fintrellis.Services.Tests.Validators
{
    public class PostCreateRequestValidatorTests
    {
        private readonly PostCreateRequestValidator _validator;

        public PostCreateRequestValidatorTests()
        {
            _validator = new PostCreateRequestValidator();
        }

        [Fact]
        public void Should_HaveError_When_TitleIsEmpty()
        {
            var model = new PostCreateRequest { Title = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Title).WithErrorMessage("Title is required.");
        }

        [Fact]
        public void Should_HaveError_When_ContentIsEmpty()
        {
            var model = new PostCreateRequest { Content = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Content).WithErrorMessage("Content is required.");
        }

        [Fact]
        public void Should_HaveError_When_PublishedDateIsInFuture()
        {
            var model = new PostCreateRequest { PublishedDate = DateTime.UtcNow.AddDays(1) };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.PublishedDate).WithErrorMessage("Published date cannot be in the future.");
        }

        [Fact]
        public void Should_HaveError_When_AuthorIsEmpty()
        {
            var model = new PostCreateRequest { Author = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Author).WithErrorMessage("Author is required.");
        }

        [Fact]
        public void Should_NotHaveAnyErrors_When_AllFieldsAreValid()
        {
            var model = new PostCreateRequest
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